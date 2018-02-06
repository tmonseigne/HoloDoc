using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;
using System.Linq;

public class CameraStream : MonoBehaviour {

    public static CameraStream Instance;
    public Texture2D substituableFrame;
    public bool forceSubstitute = false;

    private WebCamTexture cameraFrame;
    private Color32[] data;

    private int frameWidth = 0;
    private int frameHeight = 0;
    private Resolution resolution;

    private Texture2D frame;

    public Resolution Resolution
    {
        get
        {
            return resolution;
        }
    }

    public Texture2D Frame
    {
        get {
            frame.SetPixels32(data);
            frame.Apply(true);
            return frame;
        }
    }

    // Use this for initialization
    void Start()
    {
        // Top quality code !
        // TODO: Clean this ASAP
        if (Instance)
        {
            DestroyImmediate(this);
        }

        // If no substituable frame is provided, we can not force to substitute
        // TODO: Create a custom attribute class to deal with this
        if (substituableFrame == null)
        {
            forceSubstitute = false;
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        // If there is a camera and we do not force substitution
        if (devices.Length > 0 && !forceSubstitute)
        {
            try
            {
                Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
                cameraFrame = new WebCamTexture();
                cameraFrame.Play();
                frameWidth = cameraFrame.width;
                frameHeight = cameraFrame.height;
                data = cameraFrame.GetPixels32();
            } catch (System.Exception)
            {
                // If the camera failed to get a frame and we have a substitution frame
                if (substituableFrame)
                {
                    frameWidth = substituableFrame.width;
                    frameHeight = substituableFrame.height;
                    data = substituableFrame.GetPixels32();
                } else
                {
                    throw new System.Exception("No camera / substituable frame found !");
                }
            }
        }
        // If we have a substitution frame and we force substitution
        else if (substituableFrame)
        {
            frameWidth = substituableFrame.width;
            frameHeight = substituableFrame.height;
            data = substituableFrame.GetPixels32();
        } else {
            throw new System.Exception("No camera / substituable frame found !");
        }
        resolution = new Resolution()
        {
            width = frameWidth,
            height = frameHeight,
        };

        frame = new Texture2D(frameWidth, frameHeight);
        Instance = this;
    }

    // Update is called once per frame
    void Update () {
        if (substituableFrame == null || (substituableFrame && !forceSubstitute))
        {
            cameraFrame.GetPixels32(data);
        } 
    }
}
