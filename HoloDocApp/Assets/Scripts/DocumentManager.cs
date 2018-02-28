using UnityEngine;

using HoloToolkit.Unity.InputModule;

public class DocumentManager : MonoBehaviour, IInputClickHandler, IInputHandler {

	public GameObject	DocumentInformationsPrefab;
	public Color		Color;

	private Material			material;
	private Texture2D			photoTex;
	private CameraFrame			photo;
	private DocumentMesh		mesh;
	private DocumentProperties	properties;
	private GameObject			informations;

	// Use this for initialization
	void Start() {
		material = this.GetComponent<Renderer>().material;
		properties = this.GetComponent<DocumentProperties>();
		mesh = this.GetComponent<DocumentMesh>();

		informations = Instantiate(DocumentInformationsPrefab, 
								   new Vector3(mesh.Centroid.x, mesh.Centroid.y, mesh.Centroid.z - 0.2f), 
								   this.transform.rotation);
		informations.SetActive(false);

		this.SetColor(Color);
	}

	public void SetColor(Color color) {
		material.SetColor("_OutlineColor", color);
	}

	public void OnInputClicked(InputClickedEventData eventData) {
		// If the document is already photographied, toogle the informations
		if (properties.Photographied) {
			informations.SetActive(!informations.activeInHierarchy);
			if (informations.activeInHierarchy) {
				informations.GetComponent<InformationManager>().UpdateInformations(this.properties);
			}
		}
		else {
			properties.Photographied = true;
			//PhotoTaker.Instance.Photo(OnPhotoTaken);
		}
	}

	public void OnInputDown(InputEventData eventData) {
		LinkManager.Instance.OnLinkStarted(this.gameObject);
	}

	public void OnInputUp(InputEventData eventData) {
		LinkManager.Instance.OnLinkEnded(this.gameObject);
	}

	private void OnPhotoTaken(CameraFrame result) {
		photo = result;

		// Debug lines (Only used to draw result on a quad)
		photoTex.SetPixels32(photo.Data);
		photoTex.Apply(true);

		RequestLauncher.Instance.DetectDocuments(photoTex, null);
	}
}