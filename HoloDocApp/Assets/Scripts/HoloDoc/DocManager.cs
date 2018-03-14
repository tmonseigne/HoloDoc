using UnityEngine;
using System.Collections;
using HoloToolkit.Unity.InputModule;

public class DocManager : MonoBehaviour, IInputHandler, IInputClickHandler, IFocusable {

	[HideInInspector]
	public DocProperties Properties;
	public GameObject OutlineQuad;

	private GameObject docPreview, docInformations;
	private Material material;
	private DocAnimator animator;


	void Awake() {
		docPreview = transform.Find("DocPreview").gameObject;
		docInformations = transform.Find("DocInformations").gameObject;
		docInformations.SetActive(false);

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
			finalHeight = 1;
			finalWidth = (float)photo.width / (float)photo.height;
		}
		else {
			finalHeight = (float)photo.height / (float)photo.width;
			finalWidth = 1;
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
		animator.PerformAnimation();
	}

	public void Toggle() {
		docPreview.SetActive(!docPreview.activeInHierarchy);
	}

}