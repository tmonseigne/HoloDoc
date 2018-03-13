using HoloToolkit.Unity;

using System.Collections.Generic;

using UnityEngine;

public class DocumentPanel : Singleton<DocumentPanel> {
	public GameObject DocumentPrefab;
	public int MaxDocumentsPerLine = 8;

	private int lines = 0;
	private List<GameObject> documents = new List<GameObject>();

	private GameObject activeDocument = null;

	public void AddDocument(Texture2D croppedPhoto) {
		int width = croppedPhoto.width;
		int height = croppedPhoto.height;

		float finalHeight, finalWidth;
		if (height > width) {
			finalHeight = 0.2f;
			finalWidth = ((float)width / (float)height) * 0.2f;
		} else {
			finalHeight = ((float)height / (float)width) * 0.2f;
			finalWidth = 0.2f;
		}

		GameObject newDocument = Instantiate(DocumentPrefab, this.transform);
		newDocument.transform.localScale = new Vector3(finalWidth, finalHeight, 1);
		newDocument.transform.position += new Vector3(0.25f * (documents.Count % MaxDocumentsPerLine), -0.25f * lines, 0);
		newDocument.GetComponent<DocumentManage>().SetPhoto(croppedPhoto);

		documents.Add(newDocument);
		if (documents.Count % MaxDocumentsPerLine == 0) {
			lines++;
		}
	}

	public void SetActiveDocument(GameObject newActiveDocument) {
		if (this.activeDocument == null) {
			this.activeDocument = newActiveDocument;
		} else {
			if (this.activeDocument != newActiveDocument) {
				// Close the actual document
				this.activeDocument.GetComponent<DocumentAnimator>().PerformAnimation();
				this.activeDocument = newActiveDocument;
			} else {
				this.activeDocument = null;
			}
		}
	}
}
