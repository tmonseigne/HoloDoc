//using HoloToolkit.Unity.Buttons;
using HoloToolkit.Unity.InputModule;

using UnityEngine;

public class BreakLinkButtonAction : MonoBehaviour, IInputClickHandler, ISpeechHandler {
	/* 
	// TODO: Refactor BreakLinkButtonAction using button event (and so, delete IInputClickHandler and ISpeechHandler legacy)
	void Start() {
		GetComponent<Button>().OnButtonClicked += OnButtonClicked;
	}

	private void OnButtonClicked(GameObject go) {
		go.transform.parent.parent.GetComponent<DocumentManager>().UpdatePhoto();
	}
	*/

	public void OnInputClicked(InputClickedEventData eventData) {
		BreakLinks();
	}

	public void OnSpeechKeywordRecognized(SpeechEventData eventData) {
		BreakLinks();
	}

	public void BreakLinks() {
		LinkManager.Instance.BreakLink(this.transform.parent.parent.gameObject);
	}
}