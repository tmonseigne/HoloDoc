﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class DocumentDetectionTest : MonoBehaviour {

    public GameObject quad;
    private Texture2D renderTexture;

    // Use this for initialization
    void Start ()
    {
        Resolution frameResolution = CameraStream.Instance.Resolution;
        renderTexture = new Texture2D(frameResolution.width, frameResolution.height, TextureFormat.RGB24, false);
    }
	
	// Update is called once per frame
	void Update ()
    {
        Frame frame = CameraStream.Instance.Frame;
        Color32[] image = frame.Data;
        uint width = (uint)frame.Resolution.width;
        uint height = (uint)frame.Resolution.height;
        
        byte[] result = new byte[width * height * 3];

        double duration = 0;
        unsafe
        {
            duration = OpenCVInterop.SimpleDocumentDetection(ref image[0], width, height, ref result[0]);
        }
        
        renderTexture.LoadRawTextureData(result);
        renderTexture.Apply(true);

        Renderer renderer = quad.GetComponent<Renderer>();
        renderer.material.mainTexture = renderTexture;
        
        Debug.Log(duration * 1000 + "ms");
    }
}

// Define the functions which can be called from the .dll.
internal static class OpenCVInterop
{
    [DllImport("DocDetector")]
    internal unsafe static extern int DocumentDetection(uint width, uint height, ref Color32 image, uint maxDocumentsCount, ref uint outDocumentsCount, ref int outDocumentsCorners);

    [DllImport("DocDetector")]
    internal unsafe static extern double SimpleDocumentDetection(ref Color32 image, uint width, uint height, ref byte result);
}