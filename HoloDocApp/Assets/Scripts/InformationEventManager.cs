using System;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.UI.Keyboard;
using TMPro;

public class InformationEventManager : MonoBehaviour, IInputClickHandler {

	private InformationManager _infoManager;
	private GameObject _infoMax;
	private TextMeshPro _currentField = null;

	void Awake() {
		_infoManager = this.transform.root.GetComponent<InformationManager>();
		Keyboard.Instance.OnTextSubmitted += KeyboardOnTextSubmitted;
		Keyboard.Instance.OnClosed += KeyboardOnClosed;
	}

	private void KeyboardOnClosed(object sender, EventArgs e) {
		// It is really important to unsubscribe to these events as soon as the text is submitted/keyboard closed
		// otherwise modifications will be propagated to the other edited fields.
		Keyboard.Instance.OnTextUpdated -= KeyboardOnTextUpdated;
		Keyboard.Instance.OnClosed -= KeyboardOnClosed;
		Keyboard.Instance.Close();
	}

	private void KeyboardOnTextSubmitted(object sender, EventArgs e) {
		// It is really important to unsubscribe to these events as soon as the text is submitted/keyboard closed 
		// otherwise modifications will be propagated to the other edited fields.
		Keyboard.Instance.OnTextUpdated -= KeyboardOnTextUpdated;
		Keyboard.Instance.OnClosed -= KeyboardOnClosed;
	}

	private void KeyboardOnTextUpdated(string content) {
		if (!string.IsNullOrEmpty(content)) {
			_currentField.text = content;
		}
	}

	public void OnInputClicked(InputClickedEventData eventData) {
		if (eventData.selectedObject == null) {
			return;
		}
		else {
			string editField = eventData.selectedObject.name;
			bool edit = true;
			bool date = false;
			switch (editField) {
				case "ShowButton":
					edit = false;
					ToogleShow();
					break;
				case "CloseButton":
					edit = false;
					Close();
					break;
				case "Label":
					_currentField = _infoManager.label;
					break;
				case "Author":
					_currentField = _infoManager.author;
					break;
				case "Date":
					date = true;
					_currentField = _infoManager.date;
					break;
				case "Description":
					_currentField = _infoManager.description;
					break;
			}

			if (edit) {
				// We need to subscribe to this event after we set the variable currentField otherwise it is not working (???)
				Keyboard.Instance.OnTextUpdated += KeyboardOnTextUpdated;
				if (date)
					Keyboard.Instance.PresentKeyboard(_currentField.text, Keyboard.LayoutType.Symbol);
				else
					Keyboard.Instance.PresentKeyboard(_currentField.text);
			}
		}
	}

	public void Close() {
		if (_infoMax == null) {
			_infoMax = this.transform.Find("../InfoMax").gameObject;
		}

		_infoMax.SetActive(false);
		this.transform.root.gameObject.SetActive(false);
	}

	public void ToogleShow() {
		if (_infoMax == null) {
			_infoMax = this.transform.Find("../InfoMax").gameObject;
		}

		_infoMax.SetActive(!_infoMax.activeInHierarchy);
		//this.GetComponent<ButtonIconProfileTexture>().GetIcon("ChevronUp", this.GetComponent<MeshRenderer>(), this.GetComponent<MeshFilter>(), true);
	}
}