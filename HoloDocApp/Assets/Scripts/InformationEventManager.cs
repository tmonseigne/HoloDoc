using System;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.UI.Keyboard;
using TMPro;

public class InformationEventManager : MonoBehaviour, IInputClickHandler {

	private InformationManager iManager;
	private GameObject infoMax;
	private TextMeshPro currentField = null;
	private string test;

	void Awake()
	{
		iManager = this.transform.root.GetComponent<InformationManager>();
		Keyboard.Instance.OnTextSubmitted += KeyboardOnTextSubmitted;
		Keyboard.Instance.OnClosed += KeyboardOnClosed;
	}

	private void KeyboardOnClosed(object sender, EventArgs e)
	{
		// It is really important to unsubscribe to these events as soon as the text is submitted/keyboard closed
		// otherwise modifications will be propagated to the other edited fields.
		Keyboard.Instance.OnTextUpdated -= KeyboardOnTextUpdated;
		Keyboard.Instance.OnClosed -= KeyboardOnClosed;
		Keyboard.Instance.Close();
	}

	private void KeyboardOnTextSubmitted(object sender, EventArgs e)
	{
		// It is really important to unsubscribe to these events as soon as the text is submitted/keyboard closed 
		// otherwise modifications will be propagated to the other edited fields.
		Keyboard.Instance.OnTextUpdated -= KeyboardOnTextUpdated;
		Keyboard.Instance.OnClosed -= KeyboardOnClosed;
	}

	private void KeyboardOnTextUpdated(string content)
	{
		if (!string.IsNullOrEmpty(content))
		{
			currentField.text = content;
		}
	}

	public void OnInputClicked(InputClickedEventData eventData)
	{
		if(eventData.selectedObject == null)
		{
			return;
		}
		else
		{
			string editField = eventData.selectedObject.name;
			bool edit = true;
			bool date = false;
			switch (editField)
			{
				case "ShowButton":
					edit = false;
					ToogleShow();
					break;
				case "CloseButton":
					edit = false;
					Close();
					break;
				case "Label":
					currentField = iManager.label;
					break;
				case "Author":
					currentField = iManager.author;
					break;
				case "Date":
					date = true;
					currentField = iManager.date;
					break;
				case "Description":
					currentField = iManager.description;
					break;
			}

			if (edit)
			{
				// We need to subscribe to this event after we set the variable currentField otherwise it is not working (???)
				Keyboard.Instance.OnTextUpdated += KeyboardOnTextUpdated;
				if (date)
					Keyboard.Instance.PresentKeyboard(currentField.text, Keyboard.LayoutType.Symbol);
				else
					Keyboard.Instance.PresentKeyboard(currentField.text);
			}
		}
	}

	public void Close()
	{
		if (infoMax == null)
		{
			infoMax = this.transform.Find("../InfoMax").gameObject;
		}
		infoMax.SetActive(false);
		this.transform.root.gameObject.SetActive(false);
	}

	public void ToogleShow()
	{
		if (infoMax == null)
		{
			infoMax = this.transform.Find("../InfoMax").gameObject;
		}
		infoMax.SetActive(!infoMax.activeInHierarchy);
		//this.GetComponent<ButtonIconProfileTexture>().GetIcon("ChevronUp", this.GetComponent<MeshRenderer>(), this.GetComponent<MeshFilter>(), true);
	}
}
