using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InformationManager : MonoBehaviour {

	private TextMeshPro label;
	private TextMeshPro author;
	private TextMeshPro description;
	private TextMeshPro date;

	void Awake()
	{
		label = transform.Find("Label").gameObject.GetComponent<TextMeshPro>();
		author = transform.Find("InfoMax/Author").gameObject.GetComponent<TextMeshPro>();
		description = transform.Find("InfoMax/Description").gameObject.GetComponent<TextMeshPro>();
		date = transform.Find("InfoMax/Date").gameObject.GetComponent<TextMeshPro>();

		// Hide full informations in the first place
		transform.Find("InfoMax").gameObject.SetActive(false);
	}

	public void UpdateInformations(DocumentProperties props)
	{
		if (label != null)
		{
			label.text = props.label;
		}
		if (description != null)
		{
			description.text = props.description;
		}
		if (author != null)
		{
			author.text = props.author;
		}
		if (date != null)
		{
			date.text = props.date;
		}
	}
}
