using UnityEngine;

public class DocumentProperties : MonoBehaviour {

	public string label { get; set; }
	public string author { get; set; }
	public string description { get; set; }
	public string date { get; set; }
	public int linkId { get; set; }
	public bool photographied { get; set; }

	public int _id { get; set; }

	void Awake() {
		this._id = this.GetHashCode();
		this.label = "label";
		this.author = "author";
		this.description = "description";
		this.date = "date";
		this.photographied = false;
		this.linkId = -1;
	}

	void Start() {
	}

	public void SetProperties(string label, string author, string description, string date) {
		this.label = label;
		this.author = author;
		this.description = description;
		this.date = date;
	}

	override public string ToString() {
		return "Unique ID: " + this._id + "\nauthor: " + this.author + "\nlabel: " + this.label + "\ndescription: " +
		       description + "\ndate: " + date + "\nphoto available: " + photographied;
	}
}