using UnityEngine;
using System.Collections;
using HoloToolkit.Unity.InputModule;
using System.Collections.Generic;

public class DocManager : MonoBehaviour, IInputHandler, IInputClickHandler, IFocusable {

	[HideInInspector]
	public DocProperties Properties;
	public GameObject OutlineQuad;

	private GameObject docPreview, docInformations, docButtons;
	private Material material;
	private DocAnimator animator;


	void Awake() {
		docPreview = transform.Find("Preview").gameObject;
		docInformations = transform.Find("Informations").gameObject;
		docInformations.SetActive(false);
		docButtons = transform.Find("Buttons").gameObject;
		docButtons.SetActive(false);

		Properties = new DocProperties();

		animator = this.transform.GetComponent<DocAnimator>();
	}

	void Start() {
		docInformations.GetComponent<InfoManager>().SetProperties(this.Properties);
		docInformations.GetComponent<InfoManager>().OnInformationModified += DocumentInformationsModifiedHandler;

		StartCoroutine(WaitForInstantiate());
	}

	IEnumerator WaitForInstantiate() {
		yield return new WaitForSeconds(0.1f);
		DocumentPanel.Instance.SetFocusedDocument(this.gameObject);
	}

	public void SetPosition(Vector3 position) {
		this.transform.position = position;
	}

	public void SetPhoto(Texture2D photo) {
		float finalHeight, finalWidth;
		if (photo.height > photo.width)	{
			finalHeight = 1.35f;
			finalWidth = ((float)photo.width / (float)photo.height) * 1.35f;
		}
		else {
			finalHeight = ((float)photo.height / (float)photo.width) * 1.35f;
			finalWidth = 1.35f;
		}

		this.docPreview.transform.localScale = new Vector3(finalWidth, finalHeight, 1);
		this.docPreview.transform.GetComponent<Renderer>().material.mainTexture = photo;

		Resolution resolution = new Resolution() {
			width = photo.width,
			height = photo.height
		};

		CameraFrame cameraFrame = new CameraFrame(resolution, photo.GetPixels32());
		this.Properties.Photo = cameraFrame;
	}

	public void SetColor(Color color) {
		this.OutlineQuad.SetActive(true);
		this.OutlineQuad.GetComponent<Renderer>().material.color = color;
		UpdateLinkDisplay();
	}

	private void DocumentInformationsModifiedHandler(string author, string date, string description, string label) {
		this.Properties.Author = author;
		this.Properties.Date = date;
		this.Properties.Description = description;
		this.Properties.Label = label;
		//RequestLauncher.Instance.UpdateDocumentInformations(this.properties);
	}

	public void OnInputDown(InputEventData eventData) {
		StartLink();
	}

	public void OnInputUp(InputEventData eventData) {
		EndLink();
	}

	public void OnInputClicked(InputClickedEventData eventData) {
		DocumentPanel.Instance.SetFocusedDocument(this.gameObject);
	}

	public void OnFocusEnter() {
		animator.ZoomIn();
	}

	public void OnFocusExit() {
		animator.ZoomOut();
	}

	public void ToggleFocus() {
		docInformations.SetActive(!docInformations.activeInHierarchy);
		docButtons.SetActive(!docButtons.activeInHierarchy);
		UpdateLinkDisplay();
		animator.PerformAnimation();
	}

	public void UpdateLinkDisplay() {
		docInformations.transform.Find("LinkPreview/DocPreview1").gameObject.SetActive(false);
		docInformations.transform.Find("LinkPreview/DocPreview2").gameObject.SetActive(false);
		docInformations.transform.Find("LinkPreview/DocPreview3").gameObject.SetActive(false);

		if (this.Properties.LinkId != -1) {
			docInformations.transform.Find("LinkPreview/NoLinks").gameObject.SetActive(false);
			uint linkCount = 0;
			List<GameObject> objects = DocLinkManager.Instance.GetObjects(this.Properties.LinkId);
			foreach (GameObject go in objects) {
				if (go != this.gameObject) {
					linkCount++;
					if (linkCount > 3) { break; }
					GameObject preview = docInformations.transform.Find("LinkPreview/DocPreview" + linkCount).gameObject;
					preview.SetActive(true);
					preview.GetComponent<Renderer>().material.mainTexture = go.transform.Find("Preview").gameObject.GetComponent<Renderer>().material.mainTexture;
				}
			}
		}
		else {
			docInformations.transform.Find("LinkPreview/NoLinks").gameObject.SetActive(true);
		}
	}

	public void Toggle() {
		docPreview.SetActive(!docPreview.activeInHierarchy);
	}

	public void RetakePhoto() {
		DocumentPanel.Instance.Toggle();
		/*/
		PhotoCapturer.Instance.TakePhoto(OnPhotoRetaken);
		/*/
		Texture2D tex = Resources.Load<Texture2D>("Images/MultiDoc - black background");
		this.SetPhoto(tex);
		StartCoroutine(Wait());
		/**/
	}

	IEnumerator Wait() {
		yield return new WaitForSeconds(1.5f);
		DocumentPanel.Instance.Toggle();
		DocumentPanel.Instance.SetFocusedDocument(this.transform.gameObject);
	}

	public void OnPhotoRetaken(Texture2D photo, Resolution res) {
		//Texture2D newCroppedPhoto = RequestLauncher.Instance.UpdatePhoto(photo);
		this.SetPhoto(photo);
		DocumentPanel.Instance.Toggle();
	}

	public void StartLink() {
		DocLinkManager.Instance.OnLinkStarted(this.gameObject);
	}


	public void EndLink() {
		DocLinkManager.Instance.OnLinkEnded(this.gameObject);
	}

	public void OnLinkBreak() {
		this.OutlineQuad.SetActive(false);
		UpdateLinkDisplay();
		// NOTE: Maybe put a default color.
		//RequestLauncher.Instance.BreakLink(this.Properties.Id);
	}

	public void Open() {
		if (DocumentPanel.Instance.IsFocused(this.gameObject)) {
			return;
		}
		docInformations.SetActive(true);
		docButtons.SetActive(true);
		animator.OpenAnimation();
	}

	public void Close() {
		if (!DocumentPanel.Instance.IsFocused(this.gameObject)) {
			return;
		}
		docInformations.SetActive(false);
		docButtons.SetActive(false);
		animator.CloseAnimation();
	}

}