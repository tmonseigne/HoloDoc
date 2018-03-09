using UnityEngine;

public class DocumentProperties {

	public int			Id				{ get; set; }
	public int			LinkId			{ get; set; }
	public string		Label			{ get; set; }
	public string		Author			{ get; set; }
	public string		Description		{ get; set; }
	public string		Date			{ get; set; }
	public bool			Photographied	{ get; set; }
	public int[]		Corners			{ get; set; }
	public CameraFrame	Photo			{ get; set; }

	public DocumentProperties() { 
		this.Id = this.GetHashCode();
		this.LinkId = -1;
		this.Label = "undefined label";
		this.Author = "undefined author";
		this.Description = "undefined description";
		this.Date = "00/00/0000";
		this.Photographied = false;
	}

	public void SetProperties(string label, string author, string description, string date) {
		this.Label = label;
		this.Author = author;
		this.Description = description;
		this.Date = date;
	}

	// TODO: Set id to bdd id.

	override public string ToString() {
		return "Unique ID: " + Id + "\nauthor: " + Author + "\nlabel: " + Label + "\ndescription: " +
		       Description + "\ndate: " + Date + "\nphoto available: " + Photographied;
	}
}