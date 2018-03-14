using UnityEngine;
using UnityEngine.SceneManagement;

using HoloToolkit.Unity.InputModule;

public class EnterButton : MonoBehaviour, IInputClickHandler {

	public void OnInputClicked(InputClickedEventData data) {
		SceneManager.LoadScene("HoloDoc");
	}
}