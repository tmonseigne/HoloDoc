using HoloToolkit.Unity;

using System.Collections.Generic;

using UnityEngine;

public class DocumentCollection : Singleton<DocumentCollection> {

	public GameObject	DocumentPrefab;
	public int			MaxDocumentsPerLine = 4;

	private List<GameObject> documents = new List<GameObject>();

    public List<GameObject> Documents
    {
        get
        {
            return documents;
        }
    }

    private bool		isActive = false;
	private GameObject	focusedDocument = null;

	private float distancePanel = 1.5f;
	private float offsetX = 0.17f;
	private float offsetY = -0.12f;

	public void AddDocument(Texture2D croppedPhoto, string documentId) {
		Toggle();

		GameObject newDocument = Instantiate(DocumentPrefab, this.transform);
		newDocument.GetComponent<DocumentManager>().SetPosition(GetPosition(documents.Count));
		newDocument.GetComponent<DocumentManager>().SetPhoto(croppedPhoto);
		newDocument.GetComponent<DocumentManager>().Properties.Id = documentId;

		this.documents.Add(newDocument);
	}

	public Vector3 GetPosition(int documentNumber) {
		return this.transform.position +
			   this.transform.right * (offsetX * (documentNumber % MaxDocumentsPerLine)) +
			   this.transform.up * (offsetY * Mathf.Floor(documentNumber / MaxDocumentsPerLine));
	}

	public void SetFocusedDocument(GameObject newfocusedDocument) {
		GameObject previousFocusedDocument = this.focusedDocument;

		this.focusedDocument = (previousFocusedDocument != newfocusedDocument) ? newfocusedDocument : null;

		if (previousFocusedDocument != null) {
			previousFocusedDocument.GetComponent<DocumentManager>().ToggleFocus();
		}

		if (this.focusedDocument != null) {
			this.focusedDocument.GetComponent<DocumentManager>().ToggleFocus();
		}
	}

	public bool IsActive() {
		return isActive;
	}

	public int DocumentsCount() {
		return documents.Count;
	}

	public void Toggle() {
		this.isActive = !this.isActive;

		// Reposition the collection
		if (this.isActive) {
			// Reset rotation
			//this.transform.rotation = Camera.main.transform.rotation;
			this.transform.right = Vector3.right;

			Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * distancePanel;
			Vector3 directionToTarget = Camera.main.transform.position - position;
			Quaternion rotation = Quaternion.LookRotation(-directionToTarget/*, this.transform.up*/);

			// Center the panel
			position -= Camera.main.transform.right * (offsetX * (MaxDocumentsPerLine - 1) / 2f)
					  + Camera.main.transform.up	* (offsetY * Mathf.Floor(documents.Count / MaxDocumentsPerLine) / 2f);

			this.transform.position = position;
			this.transform.rotation = rotation;

			for (int i = 0; i < this.transform.childCount; i++) {
				this.transform.GetChild(i).GetComponent<DocumentAnimator>().InitTransform();
			}
		}

		SetFocusedDocument(null);

		for (int i = 0; i < this.transform.childCount; i++)	{
			this.transform.GetChild(i).GetComponent<DocumentManager>().Toggle();
		}

		if (isActive) {
			AudioPlayer.Instance.PlayClip(AudioPlayer.Instance.OpenCollection);
		}
		else {
			AudioPlayer.Instance.PlayClip(AudioPlayer.Instance.CloseCollection);
		}
	}

	public bool IsFocused(GameObject document) {
		return (this.focusedDocument == document);
	}
}
