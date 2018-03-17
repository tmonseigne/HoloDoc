using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class PhotoButtonActions : MonoBehaviour, IInputClickHandler, ISpeechHandler {

	public void OnInputClicked(InputClickedEventData eventData) {
		TakePhoto();
	}

	public void OnSpeechKeywordRecognized(SpeechEventData eventData) {
		TakePhoto();
	}

	public void TakePhoto() {
		this.transform.parent.parent.GetComponent<DocManager>().UpdatePhoto();
	}
}
