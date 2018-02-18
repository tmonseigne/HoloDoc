using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DocumentProperties : MonoBehaviour
{
	public string label { get; set; }
	public string author { get; set; }
	public string description { get; set; }
	public string date { get; set; }

	private int id;

	void Awake()
	{
		this.id = this.GetHashCode();
		this.label = "label";
		this.author = "author";
		this.description = "description";
		this.date = "date";
		Debug.LogFormat("Unique ID {0}", this.id);
	}

	void Start()
	{
	}

	public void SetProperties(string label, string author, string description, string date)
	{
		this.label = label;
		this.author = author;
		this.description = description;
		this.date = date;
	}

	public int GetId()
	{
		return this.id;
	}

	override public string ToString()
	{
		return "Unique ID: " + this.id + "\nauthor: " + this.author + "\nlabel: " + this.label + "\ndescription: " + description + "\ndate: " + date;
	}
}