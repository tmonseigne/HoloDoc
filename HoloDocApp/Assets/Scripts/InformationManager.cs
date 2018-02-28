using UnityEngine;

using TMPro;

public class InformationManager : MonoBehaviour {

	public TextMeshPro Label		{ get; set; }
	public TextMeshPro Author		{ get; set; }
	public TextMeshPro Description	{ get; set; }
	public TextMeshPro Date			{ get; set; }

	private GameObject additionalInformations;

	void Awake() {
		Label = transform.Find("Label").gameObject.GetComponent<TextMeshPro>();
		Author = transform.Find("InfoMax/Author").gameObject.GetComponent<TextMeshPro>();
		Description = transform.Find("InfoMax/Description").gameObject.GetComponent<TextMeshPro>();
		Date = transform.Find("InfoMax/Date").gameObject.GetComponent<TextMeshPro>();

		// Hide full informations in the first place
		additionalInformations = transform.Find("InfoMax").gameObject;
		additionalInformations.SetActive(false);
	}

	public void UpdateInformations(DocumentProperties props) {
		if (Label != null) {
			Label.text = props.Label;
		}

		if (Description != null) {
			Description.text = props.Description;
		}

		if (Author != null) {
			Author.text = props.Author;
		}

		if (Date != null) {
			Date.text = props.Date;
		}
	}
}