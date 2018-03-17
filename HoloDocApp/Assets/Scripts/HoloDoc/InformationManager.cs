using TMPro;

using UnityEngine;

public class InformationManager : MonoBehaviour {

	public TextMeshPro Label;
	public TextMeshPro Author;
	public TextMeshPro Date;
	public TextMeshPro Description;

	private DocumentProperties currentProperties;

	public delegate void OnInformationsModifiedCallback(string author, string date, string description, string label);
	public event OnInformationsModifiedCallback OnInformationModified;

	public void SetProperties(DocumentProperties properties) {
		this.currentProperties = properties;
		UpdateDisplay();
	}

	public void UpdateDisplay() {
		this.Label.text = this.currentProperties.Label;
		this.Description.text = this.currentProperties.Description;
		this.Author.text = this.currentProperties.Author;
		this.Date.text = this.currentProperties.Date;
	}

	public void InformationsChanged() {
		OnInformationModified(Author.text, Date.text, Description.text, Label.text);
	}
}