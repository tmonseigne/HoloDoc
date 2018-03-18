using HoloToolkit.Unity.InputModule;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DocumentPreviewManager : MonoBehaviour, IInputHandler, IInputClickHandler {

	private bool clicked = false;

	public void OnInputClicked(InputClickedEventData eventData) {
		CancelInvoke("StartLink");
		clicked = true;
		DocumentCollection.Instance.SetFocusedDocument(this.transform.parent.gameObject);
	}

	public void OnInputDown(InputEventData eventData) {
		/* Since a standard click is an Down then a Up, we need to delay the invoke of StartLink in case
		 * we only wanted to click 
		 */
		Invoke("StartLink", 0.2f);
	}

	public void OnInputUp(InputEventData eventData) {
		if (clicked) {
			clicked = false;
		}
		else {
			EndLink();
		}
	}

	public void StartLink() {
		LinkManager.Instance.OnLinkStarted(this.transform.parent.gameObject);
	}

	public void EndLink() {
		LinkManager.Instance.OnLinkEnded(this.transform.parent.gameObject);
	}
}
