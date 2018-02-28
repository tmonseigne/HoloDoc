using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class DocumentManager : MonoBehaviour, IFocusable, IInputClickHandler, IInputHandler {

	public GameObject documentInformationsPrefab;

	public Color color;
	public Color focusColor;
	public Color clickedColor;

	private Material _material;
	private Texture2D _photoTex;
	private CameraFrame _photo;
	private DocumentMesh _mesh;

	private DocumentProperties _properties;
	private GameObject _informations;

	// Use this for initialization
	void Start() {
		_material = this.GetComponent<Renderer>().material;
		_properties = this.GetComponent<DocumentProperties>();
		_mesh = this.GetComponent<DocumentMesh>();

		_informations = Instantiate(documentInformationsPrefab,
			new Vector3(_mesh.centroid.x, _mesh.centroid.y, _mesh.centroid.z - 0.2f), this.transform.rotation);
		_informations.SetActive(false);

		this.SetColor(color);
	}

	public void SetColor(Color color) {
		_material.SetColor("_OutlineColor", color);
	}

	public void OnFocusEnter() {
		//this.SetColor(focusColor);
	}

	public void OnFocusExit() {
		//this.SetColor(color);
	}

	public void OnInputClicked(InputClickedEventData eventData) {
		// If the document is already photographied, toogle the informations
		if (_properties.photographied) {
			_informations.SetActive(!_informations.activeInHierarchy);
			if (_informations.activeInHierarchy) {
				_informations.GetComponent<InformationManager>().UpdateInformations(this._properties);
			}
		}
		else {
			_properties.photographied = true;
			//PhotoTaker.Instance.Photo(OnPhotoTaken);
		}

		//this.SetColor(clickedColor);
	}

	public void OnInputDown(InputEventData eventData) {
		LinkManager.Instance.OnLinkStarted(this.gameObject);
	}

	public void OnInputUp(InputEventData eventData) {
		LinkManager.Instance.OnLinkEnded(this.gameObject);
	}

	private void OnPhotoTaken(CameraFrame result) {
		_photo = result;
		// Debug lines (Only used to draw result on a quad)
		_photoTex.SetPixels32(_photo.Data);
		_photoTex.Apply(true);

		RequestLauncher.Instance.DetectDocuments(_photoTex, null);
	}
}