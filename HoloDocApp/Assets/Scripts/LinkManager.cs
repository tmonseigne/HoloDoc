using System.Collections.Generic;

using UnityEngine;

using HoloToolkit.Unity;


public class LinkManager : Singleton<LinkManager> {

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
	}

	public List<Link> Links;

	private GameObject	linkHead;
	private GameObject	linkTail;
	private Color[]		linkColors;

	void Start() {
		Links = new List<Link>();
		linkColors = CreateColorList(10);
	}

	public void OnLinkStarted(GameObject document) {
		// If this is an actual document
		if (document.CompareTag("Document")) {
			linkHead = document;
		}
	}

	// TODO : Recieve a request to create link.
	// TODO : Send a request to the server with linked document ids.
	public void OnLinkEnded(GameObject document) {
		if (document.CompareTag("Document") && linkHead && (document != linkHead)) {
			linkTail = document;
			DocumentProperties head = linkHead.GetComponent<DocumentManager>().Properties;
			DocumentProperties tail = linkTail.GetComponent<DocumentManager>().Properties;
			if (head.LinkId != -1) {
				// The head has a link
				if (tail.LinkId != -1) {
					if (head.LinkId == tail.LinkId) {
						Debug.Log("Tail and head are already linked and plus they are in the same link → Over !");
					}
					else {
						Debug.Log("Tail and head are already part of different links → Instant merge of both links.");
						// The tail has a link too => fusion of the two lists into one
						int oldTailLinkId = tail.LinkId;
						// 1: We need to switch the tail link id to be the new id
						for (int i = 0; i < Links[oldTailLinkId].Objects.Count; i++) {
							// 1.1: Switch ids
							Links[oldTailLinkId].Objects[i].GetComponent<DocumentManager>().Properties.LinkId = head.LinkId;
							// 1.2: Switch colors
							Links[oldTailLinkId].Objects[i].GetComponent<DocumentManager>().SetColor(Links[head.LinkId].LinkColor);
						}

						// 2: We merge the lists
						Links[head.LinkId].AddRange(Links[oldTailLinkId]);
						// 3: We remove the merged list
						// TOCHECK: Memory leak ?
						//Links.RemoveAt(oldTailLinkId);
						Links[oldTailLinkId] = null;
					}
				}
				else {
					Debug.Log("Head has a link but tail is free → add tail to the head's link.");
					// The tail has no link
					// 1: We set the right id to the tail
					tail.LinkId = head.LinkId;
					// 2: We add the tail to the head's list
					Links[head.LinkId].Add(linkTail);
					// 3: We propagate the color
					linkTail.GetComponent<DocumentManager>().SetColor(Links[head.LinkId].LinkColor);
				}
			}
			else {
				// The head has no link
				if (tail.LinkId != -1) {
					Debug.Log("Tail has a link but head is free → add head to the tail's link.");
					// Tail has a link
					// 1: We set the right id to the head
					head.LinkId = tail.LinkId;
					// 2: We add the head to the tail's list
					Links[tail.LinkId].Add(linkHead);
					// 3: Propagate the color
					linkHead.GetComponent<DocumentManager>().SetColor(Links[tail.LinkId].LinkColor);
				}
				else {
					Debug.Log("Tail and head are free → Create a whole new link.");
					// Both has never been linked before
					// 1: Set the linkId property
					// Since Links can have holes, find the first null element.
					int linkId = Links.Count;
					for (int i = 0; i < Links.Count; i++) {
						if (Links[i] == null) {
							linkId = i;
							break;
						}
					}
					Debug.Log(linkId);
					head.LinkId = linkId;
					tail.LinkId = linkId;

					// 2: Create the link
					Link link = new Link() {
						Objects = new List<GameObject> {
						linkHead,
						linkTail
						},
						LinkColor = linkColors[linkId],
					};

					// 3: Push the new link in the big list
					AddToList(link, linkId);
					//Links.Add(link);

					// 4: Give it a color
					linkHead.GetComponent<DocumentManager>().SetColor(Links[linkId].LinkColor);
					linkTail.GetComponent<DocumentManager>().SetColor(Links[linkId].LinkColor);
				}
			}
		}

		// Final step: Clear head (and tail to keep consistency) otherwise clicking in the empty space will lead to a new link with an old head.
		linkHead = null;
		linkTail = null;

		// TO REMOVE: Only debug to check links current state
		Debug.LogFormat("{0} link(s) has been created.", GetLinksCount());
		for (int i = 0; i < Links.Count; i++) {
			if(Links[i] != null)
				Debug.LogFormat("Link[{0}]: {1} elems in it", i, Links[i].Objects.Count);
		}
	}

	void AddToList(Link l, int linkId) {
		if(linkId != Links.Count) {
			Links[linkId] = l;
		}
		else {
			Links.Add(l);
		}
	}

	public int GetLinksCount() {
		int count = 0;
		for(int i = 0; i < Links.Count; i++) {
			if(Links[i] != null) {
				count++;
			}
		}
		return count;
	}

	Color[] CreateColorList(int nbColors) {
		Color[] colors = new Color[nbColors];

		for (int i = 0; i < nbColors; i++) {
			colors[i] = Color.HSVToRGB(i / (float) nbColors, 1, 1);
		}

		return colors;
	}
}