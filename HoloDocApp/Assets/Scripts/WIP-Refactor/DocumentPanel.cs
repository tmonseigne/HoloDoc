using HoloToolkit.Unity;

using System.Collections.Generic;

using UnityEngine;

public class DocumentPanel : Singleton<DocumentPanel> {
	/* TODO :
	 *  - Fix bug onFocus (focus still enabled after we clicked on the document)
	 *  - Fix error "System.NotImplementedException()" on function "OnInputDown" and "OnInputUp"
	 *  - Fix error preview : when you take a new picture, all preview become the new picture.
	 *  - Fix rotation (when it's >180 degrees?)
	 * */

	public GameObject DocumentPrefab;
	public int MaxDocumentsPerLine = 8;

	private bool isActive = false;
	private List<GameObject> documents = new List<GameObject>();
	private int lines = 0;

	private GameObject focusedDocument = null;

	public void AddDocument(Texture2D croppedPhoto) {
		Toggle();
		GameObject newDocument = Instantiate(DocumentPrefab, this.transform);
		Vector3 position = this.transform.position + new Vector3(0.12f * (documents.Count % MaxDocumentsPerLine), -0.09f * lines, 0);
		newDocument.GetComponent<DocManager>().SetPosition(position);
		newDocument.GetComponent<DocManager>().SetPhoto(croppedPhoto);

		documents.Add(newDocument);
		if (documents.Count % MaxDocumentsPerLine == 0) {
			lines++;
		}
	}

	public void SetFocusedDocument(GameObject newfocusedDocument) {
		if (this.focusedDocument != null) {
			this.focusedDocument.GetComponent<DocManager>().ToggleFocus();
		}

		if (this.focusedDocument != newfocusedDocument) {
			this.focusedDocument = newfocusedDocument;
			this.focusedDocument.GetComponent<DocManager>().ToggleFocus();
		} else {
			this.focusedDocument = null;
		}
	}

	public bool IsActive() {
		return isActive;
	}

	public void Toggle() {
		isActive = !isActive;

		if (this.focusedDocument != null) {
			this.focusedDocument.GetComponent<DocManager>().ToggleFocus();
			this.focusedDocument = null;
		}

		for (int i = 0; i < this.transform.childCount; i++) {
			this.transform.GetChild(i).GetComponent<DocManager>().Toggle();
		}
	}
}
