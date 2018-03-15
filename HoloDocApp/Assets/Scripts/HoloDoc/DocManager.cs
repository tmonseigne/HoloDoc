using UnityEngine;
using System.Collections;
using HoloToolkit.Unity.InputModule;

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

		this.Properties = new DocProperties();

		animator = this.transform.GetComponent<DocAnimator>();
	}

	void Start() {
		docInformations.GetComponent<InfoManager>().SetProperties(this.Properties);
		docInformations.GetComponent<InfoManager>().OnInformationModified += DocumentInformationsModifiedHandler;

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
	}

	private void DocumentInformationsModifiedHandler(string author, string date, string description, string label) {
		this.Properties.Author = author;
		this.Properties.Date = date;
		this.Properties.Description = description;
		this.Properties.Label = label;
		//RequestLauncher.Instance.UpdateDocumentInformations(this.properties);
	}

	public void OnInputDown(InputEventData eventData) {
		DocLinkManager.Instance.OnLinkStarted(this.gameObject);
	}

	public void OnInputUp(InputEventData eventData) {
		DocLinkManager.Instance.OnLinkEnded(this.gameObject);
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
		docButtons.SetActive(!docButtons.activeInHierarchy);
		animator.PerformAnimation();
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

	public void OnLinkBreak() {
		this.OutlineQuad.SetActive(false);
		// NOTE: Maybe put a default color.
		//RequestLauncher.Instance.BreakLink(this.Properties.Id);
	}

}