using HoloToolkit.Unity.InputModule;

using UnityEngine;

public class SpeechManager : MonoBehaviour, ISpeechHandler {

	public void OnSpeechKeywordRecognized(SpeechEventData eventData) {
		DocumentManager manager = this.GetComponent<DocumentManager>();
		Debug.Log("Speech trigger");
		string keyword = eventData.RecognizedText.ToLower();
		switch (keyword) {
			case "break links":
				LinkManager.Instance.BreakLink(this.transform.gameObject);
				break;
			case "update photo":
				manager.UpdatePhoto();
				break;
			case "start link":
				manager.StartLink();
				break;
			case "end link":
				manager.EndLink();
				break;
			case "open":
				manager.Open();
				break;
			case "close":
				manager.Close();
				break;
		}
	}
}
