using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;
using System.Linq;

public class CameraStream : MonoBehaviour {

    public static CameraStream Instance;

    private static WebCamTexture _cameraFrame;
    static Color32[] data;

    public static Texture2D Frame
    {
        get {
            Texture2D tex = new Texture2D(_cameraFrame.width, _cameraFrame.height);
            tex.SetPixels32(data);
            tex.Apply(false);

            return tex;
        }
    } 
    
	// Use this for initialization
	void Start () {
		if (Instance)
        {
            DestroyImmediate(this);
        }


        WebCamDevice[] devices = WebCamTexture.devices;
        
        
        if (devices.Length > 0)
        {
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            _cameraFrame = new WebCamTexture();
            _cameraFrame.Play();

            data = new Color32[_cameraFrame.width * _cameraFrame.height];


            Instance = this;
        }
        else
        {
            throw new System.Exception("No Camera Found !");
        }

	}

    // Update is called once per frame
    void Update () {
        _cameraFrame.GetPixels32(data);
    }
}
