using HoloToolkit.Unity.InputModule;

using UnityEngine;

public class PhotoButtonAction : MonoBehaviour, IInputClickHandler {

	public void OnInputClicked(InputClickedEventData eventData) {
		this.transform.parent.parent.GetComponent<DocumentManager>().UpdatePhoto();
	}
}
