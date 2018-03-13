using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class GlobalInputReciever : MonoBehaviour, IInputClickHandler {

	[HideInInspector]
	public bool SingleTapped = true;
	[Tooltip("This is the delay during which the user can perform another tap and release the double tap event. By default this value is set to 0.5 seconds which is Microsoft's default.")]
	public float SingleTapDelay = 0.5f;

	private int tapCount = 0;

	public delegate void OnDoubleTapCallback();
	public delegate void OnSingleTapCallback(float delay);
	public event OnSingleTapCallback OnSingleTap;
	public event OnDoubleTapCallback OnDoubleTap;

	public void OnInputClicked(InputClickedEventData eventData) {
		tapCount++;
		if (tapCount == 1) {
			SingleTapped = true;
			// We can not perform a delayed Invoke on event methods so we are forced to call the method directly and wait inside this methods. To wait we are using a coroutine and we are passing the delay as a parameter.
			OnSingleTap(SingleTapDelay);
			// We invoke ResetTap SingleTapDelay seconds later so that wether we single or double tap, the tapCount is reset.
			Invoke("ResetTap", SingleTapDelay);
		}
		else if (tapCount == 2) {
			SingleTapped = false;
			OnDoubleTap();
		}
	}

	public void ResetTap() {
		tapCount = 0;
	}
	
}
