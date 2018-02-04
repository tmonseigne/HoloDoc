using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;
using System.Linq;

public class CameraStream : MonoBehaviour {

    public Texture2D substituableFrame;

    public static CameraStream Instance;
    private static WebCamTexture _cameraFrame;
    static Color32[] data;

    public Texture2D Frame
    {
        get {
            Texture2D tex;
            if (substituableFrame == null)
            {
                tex = new Texture2D(_cameraFrame.width, _cameraFrame.height);
                tex.SetPixels32(data);
                tex.Apply(false);
            } else
            {
                tex = new Texture2D(substituableFrame.width, substituableFrame.height);
                tex.SetPixels32(substituableFrame.GetPixels32());
                tex.Apply(true);
            }
            return tex;
        }
    }

    // Use this for initialization
    void Start()
    {
        if (Instance)
        {
            DestroyImmediate(this);
        }

        WebCamDevice[] devices = WebCamTexture.devices;

        if (substituableFrame == null)
        {
            if (devices.Length > 0)
            {
                Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
                _cameraFrame = new WebCamTexture();
                _cameraFrame.Play();

                data = new Color32[_cameraFrame.width * _cameraFrame.height];

            }
            else
            {
                throw new System.Exception("No Camera Found !");
            }
        }
        Instance = this;
    }

    // Update is called once per frame
    void Update () {
        if (substituableFrame == null)
        {
            _cameraFrame.GetPixels32(data);
        }
    }
}
