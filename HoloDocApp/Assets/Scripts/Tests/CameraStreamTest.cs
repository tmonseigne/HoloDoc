using UnityEngine;

public class CameraStreamTest : MonoBehaviour {

	private Texture2D renderTexture;

	// Use this for initialization
	void Start() {
		Resolution resolution = CameraStream.Instance.Frame.Resolution;
		renderTexture = new Texture2D(resolution.width, resolution.height);
	}

	// Update is called once per frame
	void Update() {
		renderTexture.SetPixels32(CameraStream.Instance.Frame.Data);
		renderTexture.Apply(true);
		this.gameObject.GetComponent<Renderer>().material.mainTexture = renderTexture;
	}
}