using System.Collections.Generic;

using UnityEngine;

using HoloToolkit.Unity;
using System;

public class LinkManager : Singleton<LinkManager>
{
	#region Link Structure
	public class Link
	{
		public List<GameObject> Objects { get; set; }
		public Color LinkColor { get; set; }

		// override AddRange function
		public void AddRange(Link l) {
			this.Objects.AddRange(l.Objects);
		}

		// override Add function
		public void Add(GameObject go) {
			this.Objects.Add(go);
		}

		// override Remove function
		public void Remove(GameObject go) {
			this.Objects.Remove(go);
		}
	}
	#endregion

	public List<Link> Links;

	private GameObject linkHead;
	private GameObject linkTail;

	void Start() {
		this.Links = new List<Link>();
	}

	public void OnLinkStarted(GameObject document) {
		// If this is an actual document
		if (document.CompareTag("Document")) {
			AudioPlayer.Instance.PlayClip(AudioPlayer.Instance.StartLinking);
			this.linkHead = document;
		}
	}

	// TODO : Receive a request to create link.
	// TODO : Send a request to the server with linked document ids.
	public void OnLinkEnded(GameObject document) {
		if (document.CompareTag("Document") && (this.linkHead != null) && (this.linkHead != document)) {
			this.linkTail = document;
			DocumentProperties headProperties = this.linkHead.GetComponent<DocumentManager>().Properties;
			DocumentProperties tailProperties = this.linkTail.GetComponent<DocumentManager>().Properties;

            //RequestLauncher.Instance.CreateLink(headProperties.Id, tailProperties.Id, OnLinkCreated);
			if (headProperties.LinkId != -1) {
				// The head has a link
				if (tailProperties.LinkId != -1) {
					if (headProperties.LinkId != tailProperties.LinkId) {
						// The tail has a link too => fusion of the two lists into one
						int oldTailLinkId = tailProperties.LinkId;
						// 1: We need to switch the tail link id to be the new id
						for (int i = 0; i < this.Links[oldTailLinkId].Objects.Count; i++) {
							// 1.1: Switch ids
							this.Links[oldTailLinkId].Objects[i].GetComponent<DocumentManager>().Properties.LinkId = headProperties.LinkId;
							// 1.2: Switch colors
							this.Links[oldTailLinkId].Objects[i].GetComponent<DocumentManager>().SetColor(Links[headProperties.LinkId].LinkColor);
						}

						// 2: We merge the lists
						this.Links[headProperties.LinkId].AddRange(Links[oldTailLinkId]);
						// 3: We "remove" the merged list
						this.Links[oldTailLinkId] = null;
						
						AudioPlayer.Instance.PlayClip(AudioPlayer.Instance.EndLinking);
					}
					else {
						AudioPlayer.Instance.PlayClip(AudioPlayer.Instance.BadLinking);
					}
				}
				else {
					/* The tail has no link
					 * 1: We set the right id to the tail
					 */
					tailProperties.LinkId = headProperties.LinkId;
					// 2: We add the tail to the head's list
					this.Links[headProperties.LinkId].Add(linkTail);
					// 3: We propagate the color
					linkTail.GetComponent<DocumentManager>().SetColor(Links[headProperties.LinkId].LinkColor);

					AudioPlayer.Instance.PlayClip(AudioPlayer.Instance.EndLinking);
				}
			}
			else { 
				// The head has no link
				if (tailProperties.LinkId != -1) {
					/* Tail has a link
					 * 1: We set the right id to the head
					 */
					headProperties.LinkId = tailProperties.LinkId;
					// 2: We add the head to the tail's list
					this.Links[tailProperties.LinkId].Add(linkHead);
					// 3: Propagate the color
					this.linkHead.GetComponent<DocumentManager>().SetColor(Links[tailProperties.LinkId].LinkColor);
					
					AudioPlayer.Instance.PlayClip(AudioPlayer.Instance.EndLinking);
				}
				else {
					/* Both has never been linked before
					 * 1: Set the linkId property
					 * Since Links can have holes, find the first null element.
					 */
					int linkId = this.Links.Count;
					for (int i = 0; i < this.Links.Count; i++) {
						if (this.Links[i] == null) {
							linkId = i;
							break;
						}
					}
					headProperties.LinkId = linkId;
					tailProperties.LinkId = linkId;

					// 2: Create the link
					Link link = new Link() {
						Objects = new List<GameObject> {
							this.linkHead,
							this.linkTail
						},
						LinkColor = GenerateUniqueColor((uint) linkId)
					};

					// 3: Push the new link in the big list
					AddToList(link, linkId);

					// 4: Give it a color
					this.linkHead.GetComponent<DocumentManager>().SetColor(Links[linkId].LinkColor);
					this.linkTail.GetComponent<DocumentManager>().SetColor(Links[linkId].LinkColor);

					AudioPlayer.Instance.PlayClip(AudioPlayer.Instance.EndLinking);
				}
			}
		}
		else if (linkHead == document) {
			AudioPlayer.Instance.PlayClip(AudioPlayer.Instance.BadLinking);
		}

		// Finally clear head (and tail to keep consistency) otherwise clicking in the empty space will lead to a new link with an old head.
		this.linkHead = null;
		this.linkTail = null;
	}

    private void OnLinkCreated(RequestLauncher.RequestAnswerSimple item, bool success)
    {
        if (!success)
        {
            Debug.Log(item.Error);
        }
    }

    void AddToList(Link link, int linkId) {
		if (linkId != this.Links.Count) {
			this.Links[linkId] = link;
		}
		else {
			this.Links.Add(link);
		}
	}

	public int GetLinksCount() {
		int count = 0;
		for (int i = 0; i < this.Links.Count; i++) {
			if (this.Links[i] != null) {
				count++;
			}
		}
		return count;
	}

	Color GenerateUniqueColor(uint color) {
		float degreeColor = 0.0f;
		if (color < 6) {
			 degreeColor = 60 * color;
		}
		else {
			float offset = 360.0f / 6.0f;
			int currentBinary = (int)Mathf.Log(Mathf.Floor(color / 6.0f), 2);
			degreeColor = offset / (1 << (currentBinary + 1)) + offset / (1 << currentBinary) * (color % (6 * (1 << currentBinary)));
		}

		return Color.HSVToRGB(degreeColor / 360.0f, 1, 1);
	}

	public void BreakLink(GameObject document) {
		DocumentManager manager = document.GetComponent<DocumentManager>();
		int linkId = manager.Properties.LinkId;
		if (linkId == -1) {
			return;
		}

		Link link = this.Links[linkId];
		link.Remove(document);
		manager.Properties.LinkId = -1;
		manager.OnLinkBreak();

		if (link.Objects.Count == 1) {
			manager = link.Objects[0].GetComponent<DocumentManager>();
			link.Remove(link.Objects[0]);
			manager.Properties.LinkId = -1;
			manager.OnLinkBreak();
			this.Links[linkId] = null;
		}

		AudioPlayer.Instance.PlayClip(AudioPlayer.Instance.BreakLinking);
	}

	public List<GameObject> GetObjects(int linkId) {
		return this.Links[linkId].Objects;
	}
}