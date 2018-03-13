using UnityEngine;

using TMPro;

public class InfoManager : MonoBehaviour
{
	private TextMeshPro Label, Author, Date, Description;
	private DocProperties currentProperties;

	public delegate void OnInformationsModifiedCallback(string author, string date, string description, string label);
	public event OnInformationsModifiedCallback OnInformationModified;

	void Start() {
		Label = this.transform.Find("Label").gameObject.GetComponent<TextMeshPro>();
		Author = this.transform.Find("Author").gameObject.GetComponent<TextMeshPro>();
		Date = this.transform.Find("Date").gameObject.GetComponent<TextMeshPro>();
		Description = this.transform.Find("Description").gameObject.GetComponent<TextMeshPro>();

		// Ugly... But TextMeshPro doesn't like prefab.. Need to improve it!
		this.transform.Find("Label").gameObject.transform.localPosition = new Vector3(-3f, 0, 0);
		this.transform.Find("Author").gameObject.transform.localPosition = new Vector3(-5.5f, -5, 0);
		this.transform.Find("Date").gameObject.transform.localPosition = new Vector3(8f, -5, 0);
		this.transform.Find("Description").gameObject.transform.localPosition = new Vector3(-0.64f, -8.9f, 0);
	}
	public void SetProperties(DocProperties properties) {
		this.currentProperties = properties;
		UpdateDisplay();
	}

	public void UpdateDisplay() {
		if (Label != null) {
			Label.text = this.currentProperties.Label;
		}

		if (Description != null) {
			Description.text = this.currentProperties.Description;
		}

		if (Author != null) {
			Author.text = this.currentProperties.Author;
		}

		if (Date != null) {
			Date.text = this.currentProperties.Date;
		}
	}

	public void InformationsChanged() {
		OnInformationModified(Author.text, Date.text, Description.text, Label.text);
	}
}