using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloDocAction : MonoBehaviour {

	public GameObject documentPanel;
	public Texture2D defaultTexture;
	public GameObject GlobalInputReciever;

	private bool photoMode = true;

	private GlobalInputReciever GlobalInput;
	void Start() {
		GlobalInput = GlobalInputReciever.GetComponent<GlobalInputReciever>();
		GlobalInput.OnSingleTap += SingleTap;
		GlobalInput.OnDoubleTap += DoubleTap;
	}

	void SingleTap(float delay) {
		// We need to use a coroutine to way for the delay before invoking single tap.
		// This will let some time to the user to perform a double tap.
		StartCoroutine(WaitForDoubleTap(delay));
	}

	IEnumerator WaitForDoubleTap(float delay) {
		yield return new WaitForSeconds(delay);
		if (GlobalInput.SingleTapped && photoMode) {
			Debug.Log("Single tap");
			// This should take a photo, send it to the server which will check if its valid crop and unwrap it and send it back. Then call the instanciator with this new photo a create a document and add it to the document panel.
			if (PhotoCapturer.Instance.HasFoundCamera) {
				PhotoCapturer.Instance.TakePhoto(OnPhotoTaken);
			} else {
				Resolution defaultTextureResolution = new Resolution();
				defaultTextureResolution.width = defaultTexture.width;
				defaultTextureResolution.height = defaultTexture.height;
				OnPhotoTaken(defaultTexture, defaultTextureResolution);
			}
		}
	}

	void DoubleTap() {
		// This should toogle (open/close) the document viewer panel & deactivate/activate the single tap event.
		// We need to deactivate the single tap event so that if we miss the documents, we won't take a photo while in 
		// document view mode.
		Debug.Log("Double tap");
		this.documentPanel.SetActive(!this.documentPanel.activeInHierarchy);
		this.photoMode = true;
	}

	public void OnPhotoTaken(Texture2D photo, Resolution res) {
		Texture2D croppedPhoto = photo; //RequestLauncher.Instance.MatchOrCreateDocument();
		this.documentPanel.SetActive(true);
		this.photoMode = false;
		DocumentPanel.Instance.AddDocument(croppedPhoto);
	}
}
