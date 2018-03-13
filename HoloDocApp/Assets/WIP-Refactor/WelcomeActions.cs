using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;

public class WelcomeActions : MonoBehaviour
{
	public GlobalInputReciever GlobalInput;
	public GameObject UIContainer;

	private GameObject colorFoundUI;
	private TextMeshPro textMeshPro;

	private CustomFade fadeEffect;

	private bool backgroundColorPicked = false;
	private bool processing = false;
	

	void Start() {
		GlobalInput.OnSingleTap += SingleTap;
		GlobalInput.OnDoubleTap += DoubleTap;

		fadeEffect = this.GetComponent<CustomFade>();

		textMeshPro = UIContainer.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>();
		colorFoundUI = UIContainer.transform.GetChild(1).gameObject;
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
			Destroy(GameObject.Find("InputManager"));
			Destroy(GameObject.Find("PhotoCapturer"));
			Destroy(GameObject.Find("ColorPicker"));
			SceneManager.LoadScene("NewHolodoc");
		}
	}

	public void OnPhotoTaken(Texture2D photo, Resolution resolution) {
		PersistentData.WorkspaceBackgroundColor = ColorPicker.Instance.AverageColor(photo);
		StartCoroutine(FadeEffect(PersistentData.WorkspaceBackgroundColor));
	}

	IEnumerator FadeEffect(Color backgroundColor) {
		fadeEffect.Fade(textMeshPro, 0.0f, 2.5f);
		yield return new WaitForSeconds(1.5f);

		textMeshPro.text = "If you wish to redo the operation please air tap again, otherwise just double tap to start working.";
		fadeEffect.Fade(textMeshPro, 1.0f, 2.5f);

		colorFoundUI.transform.GetChild(1).GetComponent<Renderer>().material.color = backgroundColor;
		colorFoundUI.SetActive(true);

		processing = false;
		backgroundColorPicked = true;
	}
}
