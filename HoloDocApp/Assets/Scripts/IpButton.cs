using System;

using UnityEngine;

using HoloToolkit.UI.Keyboard;
using HoloToolkit.Unity.InputModule;

public class IpButton : MonoBehaviour, IInputClickHandler {

	public static string IpAdress = "";

	void Awake() {
		Keyboard.Instance.OnTextUpdated += Keyboard_OnTextUpdated;
		Keyboard.Instance.OnClosed += Keyboard_OnClosed;
	}

	/// <summary>
	/// Delegate function for getting keyboard input
	/// </summary>
	/// <param name="newText"></param>
	private void Keyboard_OnTextUpdated(string newText) {
		if (!string.IsNullOrEmpty(newText)) {
			IpAdress = newText;
		}
	}

	/// <summary>
	/// Delegate function for getting keyboard input
	/// </summary>
	/// <param name="sender"></param>
	private void Keyboard_OnClosed(object sender, EventArgs e) {
		// Unsubscribe from delegate functions
		Keyboard.Instance.OnTextUpdated -= Keyboard_OnTextUpdated;
		Keyboard.Instance.OnClosed -= Keyboard_OnClosed;
	}


	private void Keyboard_OnTextSubmitted(object sender, System.EventArgs e) {
		Keyboard.Instance.Close();
	}

	public void OnInputClicked(InputClickedEventData data) {
		Debug.Log("Clicked");
		// Single-line textbox
		Keyboard.Instance.Close();
		Keyboard.Instance.PresentKeyboard(IpAdress);
	}
}