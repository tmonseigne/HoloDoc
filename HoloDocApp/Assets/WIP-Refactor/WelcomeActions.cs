using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;

public class WelcomeActions : MonoBehaviour
{
	public GlobalInputReciever GlobalInput;
	public GameObject colorPreview;
	public TextMeshPro message;

	private bool backgroundColorPicked = false;
	private bool processing = false;

	private CustomFade fadeEffect;
	private Renderer colorRenderer;

	void Start() {
		GlobalInput.OnSingleTap += SingleTap;
		GlobalInput.OnDoubleTap += DoubleTap;

		fadeEffect = this.GetComponent<CustomFade>();
		colorRenderer = colorPreview.GetComponent<Renderer>();
	}

	void SingleTap(float delay) {
		// We need to use a coroutine to way for the delay before invoking single tap.
		// This will let some time to the user to perform a double tap.
		StartCoroutine(WaitForDoubleTap(delay));
	}

	IEnumerator WaitForDoubleTap(float delay) {
		yield return new WaitForSeconds(delay);
		if (GlobalInput.SingleTapped && !processing) {
			processing = true;
			if (!PhotoCapturer.Instance.HasFoundCamera) {
				StartCoroutine(FadeEffect(Color.blue));
			}
			else {
				PhotoCapturer.Instance.TakePhoto(OnPhotoTaken);
			}
		}
	}

	void DoubleTap() {
		if (backgroundColorPicked && !processing) {
			SceneManager.LoadScene("HoloDoc");
		}
	}

	public void OnPhotoTaken(Texture2D photo, Resolution resolution) {
		PersistentData.WorkspaceBackgroundColor = ColorPicker.Instance.AverageColor(photo);
		StartCoroutine(FadeEffect(PersistentData.WorkspaceBackgroundColor));
	}

	IEnumerator FadeEffect(Color backgroundColor) {
		fadeEffect.Fade(message, 0.0f, 2.5f);
		colorPreview.SetActive(false);
		yield return new WaitForSeconds(1.5f);

		message.text = "This is the color we found. If you wish to redo the operation please air tap again, otherwise just double tap to start working.";
		fadeEffect.Fade(message, 1.0f, 2.5f);

		colorRenderer.material.color = backgroundColor;
		colorPreview.SetActive(true);

		processing = false;
		backgroundColorPicked = true;
	}
}
