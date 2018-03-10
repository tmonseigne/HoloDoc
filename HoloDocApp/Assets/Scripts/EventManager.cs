using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using HoloToolkit.Unity.InputModule;

using TMPro;


public class EventManager : MonoBehaviour, IInputClickHandler {
    public TextMeshPro	message;
    public GameObject	colorPreview;
    // This is set to public in order for us to be able to substitute a fake color when we do not own a camera.
    public Color backgroundColor = Color.blue;

    private CustomFade	fadeEffect;
	private Renderer	colorRenderer;
    // This is used as a security check to avoid relaunching animations and computation when the user spam click.
    private bool		processing = false;
    // This is used to unlock the double tap gesture and allow the user to go onto the next scene.
    private bool		backgroundPicked = false;
    private int			tapCount = 0;
    
    void Start() {
        fadeEffect = this.GetComponent<CustomFade>();
		colorRenderer = colorPreview.GetComponent<Renderer>();
	}

    public void OnPhotoTaken(Texture2D photo, Matrix4x4 proj, Matrix4x4 world, 
								bool projB, bool worldB, Resolution resolution) {
        // Get the camera matrixs and save them into a static class
        CustomCameraParameters.ProjectionMatrix = proj;
        CustomCameraParameters.WorldMatrix = world;
        CustomCameraParameters.Resolution = resolution;

        // Pick the background color and save it into a static class
        backgroundColor = ColorPicker.Instance.AverageColor(photo);
        PersistentData.WorkspaceBackgroundColor = backgroundColor;

        StartCoroutine(DoFade(backgroundColor));
    }

    public void OnInputClicked(InputClickedEventData eventData) {
        if (!processing) {
            tapCount++;
            if (tapCount == 1) {
                Invoke("SingleTap", 0.5f);
            }
            else if (tapCount == 2) {
                CancelInvoke("SingleTap");
                DoubleTap();
                tapCount = 0;
            }
        }
    }

    public void SingleTap() {
        tapCount = 0;
        processing = true;
        if (PhotoCapturer.Instance.HasFoundCamera) {
            PhotoCapturer.Instance.TakePhoto(OnPhotoTaken);
        }
        else {
			CustomCameraParameters.ProjectionMatrix = Camera.main.projectionMatrix;
			CustomCameraParameters.WorldMatrix = Camera.main.worldToCameraMatrix;
			CustomCameraParameters.Resolution = new Resolution {
				width = Camera.main.pixelWidth,
				height = Camera.main.pixelHeight
			};

			PersistentData.WorkspaceBackgroundColor = backgroundColor;
            StartCoroutine(DoFade(backgroundColor));
        }
    }

    public void DoubleTap() {
        if (backgroundPicked) {
            SceneManager.LoadScene("SpatialMappingTest");
        }
    }

    IEnumerator DoFade(Color backgroundColor) {
        fadeEffect.Fade(message, 0.0f, 2.5f);
        colorPreview.SetActive(false);
        yield return new WaitForSeconds(1.5f);

        message.text = "This is the color we found. If you wish to redo the operation please air tap again, otherwise just double tap to start working.";
        fadeEffect.Fade(message, 1.0f, 2.5f);

        colorRenderer.material.color = backgroundColor;
        colorPreview.SetActive(true);

        processing = false;
        backgroundPicked = true;
    }
}
