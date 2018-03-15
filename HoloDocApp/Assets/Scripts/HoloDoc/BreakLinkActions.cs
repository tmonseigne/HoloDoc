using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class BreakLinkActions : MonoBehaviour, IInputClickHandler
{
	public void OnInputClicked(InputClickedEventData eventData) {
		DocLinkManager.Instance.BreakLink(this.transform.parent.parent.gameObject);
	}
}