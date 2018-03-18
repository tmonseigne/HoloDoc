using System;

using HoloToolkit.Unity.InputModule;
using HoloToolkit.UI.Keyboard;

using TMPro;

using UnityEngine;

using UnityEngine.Windows.Speech;

public class InformationEventManager : MonoBehaviour, IInputClickHandler {
	private InformationManager informationManager;

	private TextMeshPro selectedField = null;
	private bool textSubmitted = false;
	private string defaultText;

	void Awake() {
		this.informationManager = this.transform.parent.GetComponent<InformationManager>();
	}

	private void OnKeyboardClosed(object sender, EventArgs e) {
		// It is really important to unsubscribe to these events as soon as the text is submitted/keyboard closed
		// otherwise modifications will be propagated to the other edited fields.
		if (!this.textSubmitted) {
			this.selectedField.text = this.defaultText;
		}
		else {
			this.textSubmitted = false;
		}

		Keyboard.Instance.OnTextUpdated -= OnKeyboardTextUpdated;
		Keyboard.Instance.OnTextSubmitted -= OnKeyboardTextSubmitted;
		Keyboard.Instance.OnClosed -= OnKeyboardClosed;
		PhraseRecognitionSystem.Restart();
	}

	private void OnKeyboardTextSubmitted(object sender, EventArgs e) {
		// OnKeyboardSubmitted calls OnClosed
		this.textSubmitted = true;
		this.informationManager.InformationsChanged();
	}

	private void OnKeyboardTextUpdated(string content) {
		if (!string.IsNullOrEmpty(content)) {
			this.selectedField.text = content;
			// This line should go in the OnKeyboardSubmitted but atm there is a bug
			// causing the event to be thrown too many times and the label ends up resetting for no reasons.
		}
	}

	public void OnInputClicked(InputClickedEventData eventData) {
		if (eventData.selectedObject == null) {
			return;
		}

		if (Keyboard.Instance.isActiveAndEnabled) {
			Keyboard.Instance.Close();
		}

		// Each time we open the keyboard we subscribe to events
		Keyboard.Instance.OnClosed += OnKeyboardClosed;
		Keyboard.Instance.OnTextSubmitted += OnKeyboardTextSubmitted;

		string editField = eventData.selectedObject.name;
		Keyboard.LayoutType keyboardLayout = Keyboard.LayoutType.Alpha;
		switch (editField) { 
			case "Label":
				this.selectedField = informationManager.Label;
				break;
			case "Author":
				this.selectedField = informationManager.Author;
				break;
			case "Date":
				keyboardLayout = Keyboard.LayoutType.Symbol;
				this.selectedField = informationManager.Date;
				break;
			case "Description":
				this.selectedField = informationManager.Description;
				break;
		}

		this.defaultText = selectedField.text;

		// We need to disable the SpeechRecognizer in order for the Dictation the work.
		if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running) {
			PhraseRecognitionSystem.Shutdown();
		}

		Keyboard.Instance.RepositionKeyboard(Camera.main.transform.position + Camera.main.transform.forward * 0.6f + Camera.main.transform.up * -0.1f);
		// We need to subscribe to this event after we set the variable currentField otherwise it is not working (???)
		Keyboard.Instance.OnTextUpdated += OnKeyboardTextUpdated;
		Keyboard.Instance.PresentKeyboard(this.selectedField.text, keyboardLayout);
	}
}