using UnityEngine;
using System.Collections;
using HoloToolkit.Unity.InputModule;

public class DocManager : MonoBehaviour, IInputHandler, IInputClickHandler, IFocusable {
	private GameObject docPreview, docInformations;

	private Material material;
	private DocumentProperties properties;
	private DocAnimator animator;


	void Awake() {
		docPreview = transform.Find("DocPreview").gameObject;
		docInformations = transform.Find("DocInformations").gameObject;
		docInformations.SetActive(false);

		properties = new DocumentProperties();
		docInformations.GetComponent<InfoManager>().SetProperties(this.properties);
		docInformations.GetComponent<InfoManager>().OnInformationModified += DocumentInformationsModifiedHandler;

		properties = new DocumentProperties();
		animator = this.transform.GetComponent<DocAnimator>();
	}

	void Start() {
		StartCoroutine(WaitForInstantiate());
	}

	IEnumerator WaitForInstantiate() {
		yield return new WaitForSeconds(0.1f);
		DocumentPanel.Instance.SetFocusedDocument(this.transform.gameObject);
	}

	public void SetPosition(Vector3 position) {
		this.transform.position = position;
	}

	public void SetPhoto(Texture2D photo) {
		float finalHeight, finalWidth;
		if (photo.height > photo.width)	{
			finalHeight = 1;
			finalWidth = (float)photo.width / (float)photo.height;
		}
		else {
			finalHeight = (float)photo.height / (float)photo.width;
			finalWidth = 1;
		}

		this.docPreview.transform.localScale = new Vector3(finalWidth, finalHeight, 1);
		this.docPreview.transform.GetComponent<Renderer>().material.mainTexture = photo;

		//this.properties.Photo = photo;
	}

	private void DocumentInformationsModifiedHandler(string author, string date, string description, string label) {
		this.properties.Author = author;
		this.properties.Date = date;
		this.properties.Description = description;
		this.properties.Label = label;
		RequestLauncher.Instance.UpdateDocumentInformations(this.properties);
	}

	public void OnInputDown(InputEventData eventData) {
		// Links !
		throw new System.NotImplementedException();
	}

	public void OnInputUp(InputEventData eventData) {
		// Links !
		throw new System.NotImplementedException();
	}


	public void OnInputClicked(InputClickedEventData eventData) {
		DocumentPanel.Instance.SetFocusedDocument(this.transform.gameObject);
	}

	public void OnFocusEnter() {
		animator.ZoomIn();
	}

	public void OnFocusExit() {
		animator.ZoomOut();
	}

	public void ToggleFocus() {
		docInformations.SetActive(!docInformations.activeInHierarchy);
		animator.PerformAnimation();
	}

	public void Toggle() {
		docPreview.SetActive(!docPreview.activeInHierarchy);
	}
}