using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStreamTest : MonoBehaviour {
    private Texture2D renderTexture;

	// Use this for initialization
	void Start ()
    {
        Resolution res = CameraStream.Instance.Resolution;
        renderTexture = new Texture2D(res.width, res.height);
    }
	
	// Update is called once per frame
	void Update ()
    {
        Renderer quadRenderer = this.gameObject.GetComponent<Renderer>() as Renderer;
        Frame frame = CameraStream.Instance.Frame;
        renderTexture.SetPixels32(frame.Data);
        renderTexture.Apply(true);
        quadRenderer.material.mainTexture = renderTexture;
    }
}
