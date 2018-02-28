using System.Collections.Generic;

using UnityEngine;

using HoloToolkit.Unity;

public class LinkManager : Singleton<LinkManager> {

	public List<List<GameObject>> Links;

	private GameObject	linkHead;
	private GameObject	linkTail;
	private Color[]		linkColors;

	void Start() {
		Links = new List<List<GameObject>>();
		linkColors = CreateColorList(6);
	}

	public void OnLinkStarted(GameObject document) {
		// If this is an actual document
		if (document.CompareTag("Document")) {
			linkHead = document;
			//Debug.LogFormat("Document {0} wants to be linked !", linkHead.GetComponent<DocumentProperties>().GetId());
		}
	}

	public void OnLinkEnded(GameObject document) {
		if (document.CompareTag("Document") && linkHead && (document != linkHead)) {
			linkTail = document;
			DocumentProperties head = linkHead.GetComponent<DocumentProperties>();
			DocumentProperties tail = linkTail.GetComponent<DocumentProperties>();
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
						Debug.Log(tail.LinkId);
						// 1: We need to switch the tail link id to be the new id
						for (int i = 0; i < Links[oldTailLinkId].Count; i++) {
							// 1.1: Switch ids
							Links[oldTailLinkId][i].GetComponent<DocumentProperties>().LinkId = head.LinkId;
							// 1.2: Switch colors
							Links[oldTailLinkId][i].GetComponent<DocumentManager>().SetColor(linkColors[head.LinkId]);
						}

						// 2: We merge the lists
						Links[head.LinkId].AddRange(Links[oldTailLinkId]);
						// 3: We remove the merged list
						Links.RemoveAt(oldTailLinkId);

						// Note : Since lists can not have empty places, removeAt moves everything after the removed element. 
						// TODO : Find a better way to do this (i.e move the last element in the list in the empty places and modify only the last one)
						// TODO : Find a better way to deal with the color (color switch is not that best)
						// HINT : Do a struct called Link with is a list of game object + a color.
						for (int i = 0; i < Links.Count; i++) {
							foreach (GameObject go in Links[i]) {
								go.GetComponent<DocumentProperties>().LinkId = i;
								go.GetComponent<DocumentManager>().SetColor(linkColors[i]);
							}
						}
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
					linkTail.GetComponent<DocumentManager>().SetColor(linkColors[head.LinkId]);
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
					linkHead.GetComponent<DocumentManager>().SetColor(linkColors[tail.LinkId]);
				}
				else {
					Debug.Log("Tail and head are free → Create a whole new link.");
					// Both has never been linked before
					// 1: Create the link
					List<GameObject> link = new List<GameObject> {
						linkHead,
						linkTail
					};
					// 2: Set the linkId property
					int linkId = Links.Count;
					head.LinkId = linkId;
					tail.LinkId = linkId;
					// 3: Give it a color
					linkHead.GetComponent<DocumentManager>().SetColor(linkColors[linkId]);
					linkTail.GetComponent<DocumentManager>().SetColor(linkColors[linkId]);
					// 4: Push the new link in the big list
					Links.Add(link);
				}
			}
		}

		// Final step: Clear head (and tail to keep consistency) otherwise clicking in the empty space will lead to a new link with an old head.
		linkHead = null;
		linkTail = null;

		// TO REMOVE: Only debug to check links current state
		Debug.LogFormat("{0} link(s) has been created.", Links.Count);
		int count = 0;
		foreach (List<GameObject> t in Links) {
			Debug.LogFormat("Link[{0}]: {1} elems in it", count++, t.Count);
		}
	}

	public int GetLinksCount() {
		return this.Links.Count;
	}

	Color[] CreateColorList(int nbColors) {
		Color[] colors = new Color[nbColors];

		for (int i = 0; i < nbColors; i++) {
			colors[i] = Color.HSVToRGB(i / (float) nbColors, 1, 1);
		}

		return colors;
	}
}