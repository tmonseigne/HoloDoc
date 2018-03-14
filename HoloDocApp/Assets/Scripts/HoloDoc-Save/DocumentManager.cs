using UnityEngine;

using HoloToolkit.Unity.InputModule;
using System;

public class DocumentManager : MonoBehaviour, IInputClickHandler, IInputHandler {

	public GameObject	DocumentInformationsPrefab;
	public Color		Color;

	private Material			material;
	private DocumentMesh		mesh;
	public DocumentProperties	Properties { get; set; }
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
		Properties = new DocumentProperties();
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
		
		informations.GetComponent<InformationManager>().SetProperties(this.Properties);
		informations.GetComponent<InformationManager>().OnInformationModified += DocumentInformationsModifiedHandler;
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

	private void DocumentInformationsModifiedHandler(string author, string date, string description, string label) {
		Debug.Log("Information modified detected");
		this.Properties.Author = author;
		this.Properties.Date = date;
		this.Properties.Description = description;
		this.Properties.Label = label;
		RequestLauncher.Instance.UpdateDocumentInformations(this.Properties);
	}

	public void SetColor(Color color) {
		material.SetColor("_OutlineColor", color);
	}

	public void OnInputClicked(InputClickedEventData eventData) {
		// If the document is already photographied, toogle the informations
		if (Properties.Photographied) {
			informations.SetActive(!informations.activeInHierarchy);
			if (informations.activeInHierarchy) { 
				informations.GetComponent<InformationManager>().UpdateDisplay();
			}
		}
		else {
            if (useBlinkEffect) { 
                this.SetColor(Color);
            }

            if (useMaskEffect) {
                Vector4 centroid4f = new Vector4(mesh.Centroid.x, mesh.Centroid.y, mesh.Centroid.z, 1.0f);
                this.GetComponent<Renderer>().material = PostPhotoMaterial;
                this.GetComponent<Renderer>().material.SetVector("_Centroid", centroid4f);
                material = this.GetComponent<Renderer>().material;
                this.SetColor(Color);
			}
			//PhotoTaker.Instance.Photo(OnPhotoTaken);
			Properties.Photographied = true;
		}
	}

	public void OnInputDown(InputEventData eventData) {
        if (Properties.Photographied) {
            LinkManager.Instance.OnLinkStarted(this.gameObject);
        }
	}

	public void OnInputUp(InputEventData eventData) {
        if (Properties.Photographied) {
            LinkManager.Instance.OnLinkEnded(this.gameObject);
        }
	}

	private void OnMatchOrCreateResult(DocumentProperties properties) {
		this.Properties.SetProperties(properties.Label, properties.Author, properties.Description, properties.Date);
	}

	private void OnPhotoTaken(CameraFrame photo) {
		this.Properties.Photo = photo;
		RequestLauncher.Instance.MatchOrCreateDocument(this.Properties, OnMatchOrCreateResult);
	}
    
    void Update() {
        if (!Properties.Photographied) {
            if (useBlinkEffect)
            {
                this.SetColor(visualBlinkEffect.Blink(Time.deltaTime));
            }
        }
    }
}