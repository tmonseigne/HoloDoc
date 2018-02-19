using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Link
{
	public int head;
	public int tail;
	public Color color;

	public bool Equals(Link other)
	{
		return (head == other.head &&
			tail == other.tail) ||
			(head == other.tail &&
			tail == other.head);
	}

	public bool HalfExist(Link other)
	{
		return head == other.head ||
			head == other.tail ||
			tail == other.head ||
			tail == other.tail;
	}
}

public class LinkManager : MonoBehaviour {
	public static LinkManager Instance;

	public List<Link> links;

	private GameObject linkHead;
	private GameObject linkTail;

	Color[] linkColors = new Color[] { Color.magenta, Color.yellow, Color.red, Color.gray, Color.blue, Color.cyan, Color.green };
	
	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		links = new List<Link>();
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
		if (document.CompareTag("Document") && linkHead != null && document != linkHead)
		{
			linkTail = document;
			DocumentProperties head = linkHead.GetComponent<DocumentProperties>();
			DocumentProperties tail = linkTail.GetComponent<DocumentProperties>();
			Link link = new Link
			{
				head = head.GetId(),
				tail = tail.GetId(),
			};

			bool alreadyIn = links.Exists(link.Equals);
			if (!alreadyIn)
			{
				// TODO : Find other link containing one of the two and propagate the choosen color to the other links.
				List<Link> halfLinks =  links.FindAll(link.HalfExist);
				//Debug.LogFormat("Found {0} link using same head / tail.", halfLinks.Count);
				if(halfLinks.Count > 0)
				{
					link.color = halfLinks[0].color;
				} else
				{
					link.color = linkColors[links.Count];
				}

				//Debug.LogFormat("Document {0} is now linked with Document {1} !", head.GetId(), tail.GetId());
				links.Add(link);
				// Maybe we should not do this here
				linkHead.GetComponent<DocumentManager>().SetColor(link.color);
				linkTail.GetComponent<DocumentManager>().SetColor(link.color);
			} else
			{
				//Debug.Log("Already linked");
			}
		}
	}

	public int GetLinksCount()
	{
		return this.links.Count;
	}
}
