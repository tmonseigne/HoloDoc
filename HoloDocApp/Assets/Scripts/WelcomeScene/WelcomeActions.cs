using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

public class WelcomeActions : MonoBehaviour {

	public GlobalInputReceiver GlobalInputReceiver;
	public GameObject UIContainer;

	private GameObject colorFoundUI;
	private TextMeshPro textMeshPro;

	private CustomFade fadeEffect;

	private bool backgroundColorPicked = false;
	private bool processing = false;
	

	void Start() {
		this.GlobalInputReceiver.OnSingleTap += SingleTap;
		this.GlobalInputReceiver.OnDoubleTap += DoubleTap;

		this.fadeEffect = this.GetComponent<CustomFade>();

		this.textMeshPro = UIContainer.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>();
		this.colorFoundUI = UIContainer.transform.GetChild(1).gameObject;
	}

	void SingleTap(float delay) {
		// We need to use a coroutine to way for the delay before invoking single tap.
		// This will let some time to the user to perform a double tap.
		StartCoroutine(WaitForDoubleTap(delay));
	}

	IEnumerator WaitForDoubleTap(float delay) {
		yield return new WaitForSeconds(delay);
		if (this.GlobalInputReceiver.SingleTapped && !processing) {
			this.processing = true;
			if (!PhotoCapturer.Instance.HasFoundCamera) {
				StartCoroutine(FadeEffect(Color.blue));
			}
			else {
				PhotoCapturer.Instance.TakePhoto(OnPhotoTaken);
			}
		}
	}

	void DoubleTap() {
		if (this.backgroundColorPicked && !this.processing) {
			Destroy(GameObject.Find("InputManager"));
			Destroy(GameObject.Find("PhotoCapturer"));
			Destroy(GameObject.Find("ColorPicker"));
			SceneManager.LoadScene("Scenes/HoloDoc");
		}
	}

	public void OnPhotoTaken(Texture2D photo, Resolution resolution) {
		PersistentData.WorkspaceBackgroundColor = ColorPicker.Instance.AverageColor(photo);
		StartCoroutine(FadeEffect(PersistentData.WorkspaceBackgroundColor));
	}

	IEnumerator FadeEffect(Color backgroundColor) {
		this.fadeEffect.Fade(textMeshPro, 0.0f, 2.5f);
		yield return new WaitForSeconds(1.5f);

		this.textMeshPro.text = "If you wish to redo the operation please air tap again, otherwise just double tap to start working.";
		this.fadeEffect.Fade(textMeshPro, 1.0f, 2.5f);

		this.colorFoundUI.transform.GetChild(1).GetComponent<Renderer>().material.color = backgroundColor;
		this.colorFoundUI.SetActive(true);

		this.processing = false;
		this.backgroundColorPicked = true;
	}
}
