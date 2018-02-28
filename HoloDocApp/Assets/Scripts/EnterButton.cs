using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.UI.Keyboard;
using UnityEngine.SceneManagement;

public class EnterButton : MonoBehaviour, IInputClickHandler {

	void Start() {}

	public void OnInputClicked(InputClickedEventData data) {
		Debug.Log("Enter");
		Keyboard.Instance.Close();
		SceneManager.LoadScene("HoloDoc");
	}
}