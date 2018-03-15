using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class SpeechManager : MonoBehaviour, ISpeechHandler {

	public void OnSpeechKeywordRecognized(SpeechEventData eventData) {
		Debug.Log("Speech trigger");
		string keyword = eventData.RecognizedText.ToLower();
		switch (keyword) {
			case "break links":
				DocLinkManager.Instance.BreakLink(this.transform.gameObject);
				break;
			case "take photo":
				this.GetComponent<DocManager>().RetakePhoto();
				break;
		}
	}

	void Start () {
		Debug.Log(this.GetComponent<DocManager>().name);
	}

	void Update () {
		
	}
}
