using UnityEngine;
using System.Collections;
using HoloToolkit.Unity.InputModule;

public class DocumentManage : MonoBehaviour, IInputHandler, IInputClickHandler, IFocusable
{
	public GameObject DocumentInformationPrefab;

	private Material material;
	private DocumentProperties properties;

	private GameObject informationDisplay;
	private DocumentAnimator animator;

	// Use this for initialization
	void Awake() {
		properties = new DocumentProperties();
		material = this.GetComponent<Renderer>().material;
		animator = this.GetComponent<DocumentAnimator>();
	}

	void Start() {
		informationDisplay = Instantiate(DocumentInformationPrefab, this.transform.position, this.transform.rotation);
		informationDisplay.GetComponent<InformationManager>().SetProperties(this.properties);
		informationDisplay.GetComponent<InformationManager>().OnInformationModified += DocumentInformationsModifiedHandler;
		StartCoroutine(WaitForInstantiate());
	}

	IEnumerator WaitForInstantiate() {
		yield return new WaitForSeconds(0.1f);
		animator.PerformAnimation();
		DocumentPanel.Instance.SetActiveDocument(this.gameObject);
	}

	private void DocumentInformationsModifiedHandler(string author, string date, string description, string label) {
		this.properties.Author = author;
		this.properties.Date = date;
		this.properties.Description = description;
		this.properties.Label = label;
		RequestLauncher.Instance.UpdateDocumentInformations(this.properties);
	}

	public void SetPhoto(Texture2D photo) {
		//this.properties.Photo = photo;
		material.mainTexture = photo;
	}

	public void OnInputDown(InputEventData eventData) {
		throw new System.NotImplementedException();
	}

	public void OnInputUp(InputEventData eventData) {
		throw new System.NotImplementedException();
	}

	public void SetInformationPosition(Vector3 newPos, float offset) {
		informationDisplay.transform.position = newPos + new Vector3(informationDisplay.transform.localScale.x/2 + offset, 0, 0);
	}

	public void OnInputClicked(InputClickedEventData eventData) {
		animator.PerformAnimation();
		DocumentPanel.Instance.SetActiveDocument(this.gameObject);
		this.informationDisplay.SetActive(!this.informationDisplay.activeInHierarchy);
	}

	public void OnFocusEnter() {
		animator.ZoomIn();
	}

	public void OnFocusExit() {
		animator.ZoomOut();
	}
}