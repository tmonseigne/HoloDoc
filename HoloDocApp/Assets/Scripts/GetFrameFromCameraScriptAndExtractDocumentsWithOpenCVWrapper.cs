using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class GetFrameFromCameraScriptAndExtractDocumentsWithOpenCVWrapper : MonoBehaviour {

    public GameObject quad;

    private Texture2D tex;

    // Use this for initialization
    void Start () {

        Resolution res = CameraStream.Instance.Resolution;
        tex = new Texture2D((int)res.width, (int)res.height, TextureFormat.RGB24, false);
    }
	
	// Update is called once per frame
	void Update () {
        
        Texture2D frame = CameraStream.Instance.Frame;
        Color32[] image = frame.GetPixels32();
        uint height = (uint)frame.height;
        uint width = (uint)frame.width;
        
        byte[] result = new byte[width * height * 3];

        double duration = 0;
        unsafe
        {

            duration = OpenCVInterop.SimpleDocumentDetection(ref image[0], width, height, ref result[0]);
        }
        
        tex.LoadRawTextureData(result);
        tex.Apply(true);

        Renderer renderer = quad.GetComponent<Renderer>();
        renderer.material.mainTexture = tex;
        
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