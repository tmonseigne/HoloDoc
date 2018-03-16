using UnityEngine;

using TMPro;

public class InfoManager : MonoBehaviour
{
	public TextMeshPro Label, Author, Date, Description;
	private DocProperties currentProperties;

	public delegate void OnInformationsModifiedCallback(string author, string date, string description, string label);
	public event OnInformationsModifiedCallback OnInformationModified;

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