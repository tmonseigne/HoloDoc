//using HoloToolkit.Unity.Buttons;
using HoloToolkit.Unity.InputModule;

using UnityEngine;

public class PhotoButtonAction : MonoBehaviour, IInputClickHandler, ISpeechHandler {
	/* 
	// TODO: Refactor PhotoButtonAction using button event (and so, delete IInputClickHandler and ISpeechHandler legacy)
	void Start() {
		GetComponent<Button>().OnButtonClicked += OnButtonClicked;
	}

	private void OnButtonClicked(GameObject go) {
		go.transform.parent.parent.GetComponent<DocumentManager>().UpdatePhoto();
	}
	*/

	public void OnInputClicked(InputClickedEventData eventData) {
		UpdatePhoto();
	}

	public void OnSpeechKeywordRecognized(SpeechEventData eventData) {
		UpdatePhoto();
	}

	public void UpdatePhoto() {
		this.transform.parent.parent.GetComponent<DocumentManager>().UpdatePhoto();
	}
} 