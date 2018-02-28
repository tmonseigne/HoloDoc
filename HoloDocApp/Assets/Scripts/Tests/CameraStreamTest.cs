using UnityEngine;

public class CameraStreamTest : MonoBehaviour {

	private Texture2D _renderTexture;

	// Use this for initialization
	void Start() {
		Resolution resolution = CameraStream.Instance.Frame.Resolution;
		_renderTexture = new Texture2D(resolution.width, resolution.height);
	}

	// Update is called once per frame
	void Update() {
		_renderTexture.SetPixels32(CameraStream.Instance.Frame.Data);
		_renderTexture.Apply(true);
		this.gameObject.GetComponent<Renderer>().material.mainTexture = _renderTexture;
	}
}