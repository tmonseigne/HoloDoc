using HoloToolkit.Unity;

using System.Collections.Generic;

using UnityEngine;

public class DocumentPanel : Singleton<DocumentPanel> {
	/* TODO :
	 *  - Fix bug onFocus (focus still enabled after we clicked on the document)
	 * */

	public GameObject DocumentPrefab;
	public int MaxDocumentsPerLine = 4;

	private bool isActive = false;
	private List<GameObject> documents = new List<GameObject>();
	private float distancePanel = 2.0f;
	private float offsetX = 0.17f, offsetY = -0.12f;
	private GameObject focusedDocument = null;

	public void AddDocument(Texture2D croppedPhoto) {
		Toggle();
		GameObject newDocument = Instantiate(DocumentPrefab, this.transform);
		Vector3 position = GetPosition(documents.Count);
		newDocument.GetComponent<DocManager>().SetPosition(position);
		newDocument.GetComponent<DocManager>().SetPhoto(croppedPhoto);

		documents.Add(newDocument);
	}

	public Vector3 GetPosition(int documentNumber) {
		return this.transform.position +
			   this.transform.right * (offsetX * (documentNumber % MaxDocumentsPerLine)) +
			   this.transform.up * (offsetY * Mathf.Floor(documentNumber / MaxDocumentsPerLine));
	}

	public void SetFocusedDocument(GameObject newfocusedDocument) {
		GameObject previousFocusedDocument = this.focusedDocument;

		this.focusedDocument = (this.focusedDocument != newfocusedDocument) ? newfocusedDocument : null;

		if (previousFocusedDocument != null) {
			previousFocusedDocument.GetComponent<DocManager>().ToggleFocus();
		}

		if (this.focusedDocument != null) {
			this.focusedDocument.GetComponent<DocManager>().ToggleFocus();
		}
	}

	public bool IsActive() {
		return isActive;
	}

	public int DocumentsCount() {
		return documents.Count;
	}


	public void Toggle()
	{
		isActive = !isActive;

		if (isActive) {
			// Reset rotation
			this.transform.rotation = Camera.main.transform.rotation;

			Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * distancePanel;
			Vector3 directionToTarget = Camera.main.transform.position - position;
			Quaternion rotation = Quaternion.LookRotation(-directionToTarget, this.transform.up);

			// Center the panel
			position -= Camera.main.transform.right * (offsetX * (MaxDocumentsPerLine - 1) / 2f)
					  + Camera.main.transform.up	* (offsetY * Mathf.Floor(documents.Count / MaxDocumentsPerLine) / 2f);

			this.transform.position = position;
			this.transform.rotation = rotation;

			for (int i = 0; i < this.transform.childCount; i++) {
				this.transform.GetChild(i).GetComponent<DocAnimator>().InitTransform();
			}
		}

		if (this.focusedDocument != null) {
			this.focusedDocument.GetComponent<DocManager>().ToggleFocus();
			this.focusedDocument = null;
		}

		for (int i = 0; i < this.transform.childCount; i++)
		{
			this.transform.GetChild(i).GetComponent<DocManager>().Toggle();
		}
	}

	public bool IsFocused(GameObject document) {
		return (this.focusedDocument == document);
	}
}
