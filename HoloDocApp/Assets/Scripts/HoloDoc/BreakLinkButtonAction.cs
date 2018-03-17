using HoloToolkit.Unity.InputModule;

using UnityEngine;

public class BreakLinkButtonAction : MonoBehaviour, IInputClickHandler {

	public void OnInputClicked(InputClickedEventData eventData) {
		LinkManager.Instance.BreakLink(this.transform.parent.parent.gameObject);
	}
}