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

    // Visual effect
    // Fade
    public bool             useBlinkEffect = false;
    private ColorFadeEffect visualBlinkEffect;

    // Hide
    public bool     useMaskEffect = false;
    public Material PostPhotoMaterial;
    public Material PrePhotoMaterial;

	// Use this for initialization
	void Start() {
        properties = this.GetComponent<DocumentProperties>();
        mesh = this.GetComponent<DocumentMesh>();

        if (useMaskEffect) {
            this.GetComponent<Renderer>().material = PrePhotoMaterial;
        }
        else {
            Vector4 centroid4f = new Vector4(mesh.Centroid.x, mesh.Centroid.y, mesh.Centroid.z, 1.0f);
            this.GetComponent<Renderer>().material = PostPhotoMaterial;
            this.GetComponent<Renderer>().material.SetVector("_Centroid", centroid4f);
        }

        material = this.GetComponent<Renderer>().material;

        informations = Instantiate(DocumentInformationsPrefab, 
								   new Vector3(mesh.Centroid.x, mesh.Centroid.y, mesh.Centroid.z - 0.2f), 
								   this.transform.rotation);
		informations.SetActive(false);
		this.SetColor(Color);

        // Visual effect
        visualBlinkEffect = this.GetComponent<ColorFadeEffect>();
        if (visualBlinkEffect == null) {
            useBlinkEffect = false;
        }
        else {
            visualBlinkEffect.SetSourceColor(Color);
        }
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

            if (useBlinkEffect) { 
                this.SetColor(Color);
            }

            if (useMaskEffect) {
                Vector4 centroid4f = new Vector4(mesh.Centroid.x, mesh.Centroid.y, mesh.Centroid.z, 1.0f);
                this.GetComponent<Renderer>().material = PostPhotoMaterial;
                this.GetComponent<Renderer>().material.SetVector("_Centroid", centroid4f);
                material = this.GetComponent<Renderer>().material;
                this.SetColor(Color);
                //PhotoTaker.Instance.Photo(OnPhotoTaken);
            }
        }
	}

	public void OnInputDown(InputEventData eventData) {
        if (properties.Photographied) {
            LinkManager.Instance.OnLinkStarted(this.gameObject);
        }
	}

	public void OnInputUp(InputEventData eventData) {
        if (properties.Photographied) {
            LinkManager.Instance.OnLinkEnded(this.gameObject);
        }
	}

	private void OnPhotoTaken(CameraFrame result) {
		photo = result;

		// Debug lines (Only used to draw result on a quad)
		photoTex.SetPixels32(photo.Data);
		photoTex.Apply(true);

		RequestLauncher.Instance.DetectDocuments(photoTex, null);
	}
    
    void Update() {
        if (!properties.Photographied) {
            if (useBlinkEffect)
            {
                this.SetColor(visualBlinkEffect.Blink(Time.deltaTime));
            }
        }
    }
}