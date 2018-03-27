using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ServerIpConfig : MonoBehaviour
{
	[Range(1, 30)]
	[Tooltip("How many seconds before server connection times out.")]
	public int Timeout = 5;

	[Tooltip("The Ip adress text field.")]
	public Text ipAddress;

	[Tooltip("The connection indicator image.")]
	public Image connectionIndicator;

	private bool isTryingToConnect;

	// Use this for initialization
	void Awake() {
		ipAddress.text = PersistentData.ServerIp;
	}

	public void InputString(string input) {
		this.isTryingToConnect = false;
		this.ipAddress.text += input;
	}

	public void DeleteLastCharacter() {
		this.isTryingToConnect = false;
		if (!string.IsNullOrEmpty(this.ipAddress.text)) {
			this.ipAddress.text = this.ipAddress.text.Substring(0, this.ipAddress.text.Length - 1);
		}
	}

	public void ClearIpAddressString() {
		this.isTryingToConnect = false;
		this.ipAddress.text = "";
	}

	public void ConnectIpAdress() {
		if (!this.isTryingToConnect) {
			TryConnection();
		}
	}

	public void OnConnectionSucceed() {
		this.ipAddress.text = "Connected";
		this.connectionIndicator.color = Color.green;
		this.isTryingToConnect = false;
		StartCoroutine(WaitAndSwitchScene());
	}

	public void OnConnectionFailed() {
		this.ipAddress.text = "Connection failed";
		this.connectionIndicator.color = Color.red;
		this.isTryingToConnect = false;
	}

	public void TryConnection() {
        Debug.Log("trying");
		this.isTryingToConnect = true;
		PersistentData.ServerIp = this.ipAddress.text;
		this.ipAddress.text = "Connecting";
		this.connectionIndicator.color = Color.yellow;
		RequestLauncher.Instance.Ping(OnPingRequest);
	}

	private void OnPingRequest(RequestLauncher.PingRequestData item, bool success) {
        Debug.Log("on ping");
        if (success) {
			OnConnectionSucceed();
		}
		else {
			OnConnectionFailed();
		}
	}

	IEnumerator WaitAndSwitchScene() {
		yield return new WaitForSeconds(1);
		Destroy(GameObject.Find("InputManager"));
		Destroy(GameObject.Find("RequestLauncher"));
		SceneManager.LoadScene("Scenes/HoloDoc");
	}
}
