using HoloToolkit.Unity.InputModule;

using UnityEngine;

public class PhotoButtonAction : MonoBehaviour, IInputClickHandler, ISpeechHandler {

	public void OnInputClicked(InputClickedEventData eventData) {
		TakePhoto();
	}

	public void OnSpeechKeywordRecognized(SpeechEventData eventData) {
		TakePhoto();
	}

	public void TakePhoto() {
		this.transform.parent.parent.GetComponent<DocumentManager>().UpdatePhoto();
	}
}
