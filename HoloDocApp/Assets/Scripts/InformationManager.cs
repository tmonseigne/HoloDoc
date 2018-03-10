using UnityEngine;

using TMPro;

public class InformationManager : MonoBehaviour {

	public TextMeshPro Author;
	public TextMeshPro Date;
	public TextMeshPro Description;
	public TextMeshPro Label;

	private DocumentProperties currentProperties;
	private GameObject additionalInformations;

	public delegate void OnInformationsModifiedCallback(string author, string date, string description, string label);
	public event OnInformationsModifiedCallback OnInformationModified;

	void Awake() {
		additionalInformations = transform.Find("InfoMax").gameObject;
		additionalInformations.SetActive(false);
	}

	public void SetProperties(DocumentProperties properties) {
		this.currentProperties = properties;
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