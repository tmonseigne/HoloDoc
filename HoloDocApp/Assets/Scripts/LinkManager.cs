using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LinkManager : MonoBehaviour {
	public static LinkManager Instance;

	public List<List<GameObject>> links;

	private GameObject linkHead, linkTail;
	private Color[] linkColors;

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		links = new List<List<GameObject>>();
		linkColors = CreateColorList(6);
	}

	public void OnLinkStarted(GameObject document)
	{
		// If this is an actual document
		if (document.CompareTag("Document"))
		{
			linkHead = document;
			//Debug.LogFormat("Document {0} wants to be linked !", linkHead.GetComponent<DocumentProperties>().GetId());
		}
	}

	public void OnLinkEnded(GameObject document)
	{
		if (document.CompareTag("Document") && linkHead && (document != linkHead))
		{
			linkTail = document;
			DocumentProperties head = linkHead.GetComponent<DocumentProperties>();
			DocumentProperties tail = linkTail.GetComponent<DocumentProperties>();
			if (head.linkId != -1)
			{
				// The head has a link
				if (tail.linkId != -1)
				{
					if(head.linkId == tail.linkId)
					{
						Debug.Log("Tail and head are already linked and plus they are in the same link → Over !");
						// DO NOT PUT A RETURN HERE JIMMY OTHERWISE THE LAST LINES WONT EXECUTE (clear head is necessary)
					}
					Debug.Log("Tail and head are already part of different links → Instant merge of both links.");
					// The tail has a link too => fusion of the two lists into one
					int oldTailLinkId = tail.linkId;
					// 1: We need to switch the tail link id to be the new id
					for (int i = 0; i < links[oldTailLinkId].Count; i++)
					{
						// 1.1: Switch ids
						links[oldTailLinkId][i].GetComponent<DocumentProperties>().linkId = head.linkId;
						// 1.2: Switch colors
						links[oldTailLinkId][i].GetComponent<DocumentManager>().SetColor(linkColors[head.linkId]);
					}
					// 2: We merge the lists
					links[head.linkId].Union(links[oldTailLinkId]);
					// 3: We remove the merged list
					links.RemoveAt(oldTailLinkId);
				}
				else
				{
					Debug.Log("Head has a link but tail is free → add tail to the head's link.");
					// The tail has no link
					// 1: We set the right id to the tail
					tail.linkId = head.linkId;
					// 2: We add the tail to the head's list
					links[head.linkId].Add(linkTail);
					// 3: We propagate the color
					linkTail.GetComponent<DocumentManager>().SetColor(linkColors[head.linkId]);
				}
			}
			else
			{
				// The head has no link
				if (tail.linkId != -1)
				{
					Debug.Log("Tail has a link but head is free → add head to the tail's link.");
					// Tail has a link
					// 1: We set the right id to the head
					head.linkId = tail.linkId;
					// 2: We add the head to the tail's list
					links[tail.linkId].Add(linkHead);
					// 3: Propagate the color
					linkHead.GetComponent<DocumentManager>().SetColor(linkColors[tail.linkId]);
				}
				else
				{
					Debug.Log("Tail and head are free → Create a whole new link.");
					// Both has never been linked before
					// 1: Create the link
					List<GameObject> link = new List<GameObject>
					{
						linkHead,
						linkTail
					};
					// 2: Set the linkId property
					int linkId = links.Count;
					head.linkId = linkId;
					tail.linkId = linkId;
					// 3: Give it a color
					linkHead.GetComponent<DocumentManager>().SetColor(linkColors[linkId]);
					linkTail.GetComponent<DocumentManager>().SetColor(linkColors[linkId]);
					// 4: Push the new link in the big list
					links.Add(link);
				}
			}
		}

		// Final step: Clear head (and tail to keep consistency) otherwise clicking in the empty space will lead to a new link with an old head.
		linkHead = null;
		linkTail = null;

		// TO REMOVE: Only debug to check links current state
		Debug.LogFormat("{0} link(s) has been created.", links.Count);
		int count = 0;
		foreach (List<GameObject> t in links)
		{
			Debug.LogFormat("Link[{0}]: {1} elems in it", count++, t.Count);
		}
	}

	public int GetLinksCount()
	{
		return this.links.Count;
	}

	Color[] CreateColorList(int nbColors)
	{
		Color[] colors = new Color[nbColors];

		for (int i = 0; i < nbColors; i++)
		{
			colors[i] = Color.HSVToRGB(i / (float)nbColors, 1, 1);
		}

		return colors;
	}
}
