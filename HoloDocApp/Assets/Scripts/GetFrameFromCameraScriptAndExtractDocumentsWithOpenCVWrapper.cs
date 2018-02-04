using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class GetFrameFromCameraScriptAndExtractDocumentsWithOpenCVWrapper : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Texture2D frame = CameraStream.Instance.Frame;
        Color32[] image = frame.GetPixels32();
        uint height = (uint)frame.height;
        uint width = (uint)frame.width;

        double duration = 0;
        unsafe
        {
            duration = OpenCVInterop.SimpleDocumentDetection(ref image[0], width, height);
        }

        Debug.Log(duration * 1000 + "ms");
    }
}

// Define the functions which can be called from the .dll.
internal static class OpenCVInterop
{
    [DllImport("DocDetector")]
    internal unsafe static extern int DocumentDetection(uint width, uint height, ref Color32 image, uint maxDocumentsCount, ref uint outDocumentsCount, ref int outDocumentsCorners);

    [DllImport("DocDetector")]
    internal unsafe static extern double SimpleDocumentDetection(ref Color32 image, uint width, uint height);
}