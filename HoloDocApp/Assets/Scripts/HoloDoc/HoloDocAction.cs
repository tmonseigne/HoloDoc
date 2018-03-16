using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloDocAction : MonoBehaviour {

	public GameObject	GlobalInputReciever;
	public Texture2D	DefaultTexture;

	private GlobalInputReciever GlobalInput;
	private bool photoMode = true;

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
				Resolution defaultTextureResolution = new Resolution {
					width = DefaultTexture.width,
					height = DefaultTexture.height
				};
				OnPhotoTaken(DefaultTexture, defaultTextureResolution);
			}
		}
	}

	void DoubleTap() {
		// This should toogle (open/close) the document viewer panel & deactivate/activate the single tap event.
		// We need to deactivate the single tap event so that if we miss the documents, we won't take a photo while in
		// document view mode.
		Debug.Log("Double tap");
		if (DocumentPanel.Instance.DocumentsCount() > 0) {
			DocumentPanel.Instance.Toggle();
			this.photoMode = !DocumentPanel.Instance.IsActive();
		}
	}

	public void OnPhotoTaken(Texture2D photo, Resolution res) {
		this.photoMode = false;

		/* For tests without launch the server
		Texture2D croppedPhoto = new Texture2D(photo.width, photo.height);
		croppedPhoto.SetPixels32(photo.GetPixels32());
		croppedPhoto.Apply();
		DocumentPanel.Instance.AddDocument(croppedPhoto); 
		*/
		CameraFrame frame = new CameraFrame(res, photo.GetPixels32());
		RequestLauncher.Instance.MatchOrCreateDocument(frame, OnMatchOrCreateRequest);
	}


    private void OnMatchOrCreateRequest(RequestLauncher.RequestAnswerDocument item, bool success)
    {
        if (success)
        {
            CameraFrame frame = item.CameraFrameFromBase64();
            Texture2D croppedPhoto = new Texture2D(frame.Resolution.width, frame.Resolution.height);
            croppedPhoto.SetPixels32(frame.Data);
            croppedPhoto.Apply();

            DocumentPanel.Instance.AddDocument(croppedPhoto);
        }
        else
        {
            Debug.Log(item.error);
        }
    }
}
