using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class DocumentDetectionTest : MonoBehaviour
{
	public GameObject quad;
	[Range(0,20)]
	public uint maxDocumentsCount = 5;
	private Texture2D renderTexture;

	// Use this for initialization
	void Start()
	{
		Resolution frameResolution = CameraStream.Instance.Resolution;
		renderTexture = new Texture2D(frameResolution.width, frameResolution.height, TextureFormat.RGB24, false);
	}

	// Update is called once per frame
	void Update()
	{
		CameraFrame frame = CameraStream.Instance.Frame;
		Color32[] image = frame.Data;
		uint width = (uint)frame.Resolution.width;
		uint height = (uint)frame.Resolution.height;
		uint outDocumentsCount = 0;

		byte[] result = new byte[width * height * 3];
		int[] outDocumentsCorners = new int[maxDocumentsCount * 8];

		int errCode = 1;
		unsafe
		{
			errCode = OpenCVInterop.SimpleDocumentDetection(ref image[0], width, height, ref result[0], maxDocumentsCount, ref outDocumentsCount, ref outDocumentsCorners[0]);
		}

		renderTexture.LoadRawTextureData(result);
		renderTexture.Apply(true);

		Renderer renderer = quad.GetComponent<Renderer>();
		renderer.material.mainTexture = renderTexture;

		Debug.Log("outDocumentsCount : " + outDocumentsCount);
		for (uint i = 0; i < outDocumentsCount; i++)
		{
			Debug.Log("DocumentsCorners[" + i + "]: " + 
				outDocumentsCorners[i * 8 + 0] + " " + outDocumentsCorners[i * 8 + 1] + " " + 
				outDocumentsCorners[i * 8 + 2] + " " + outDocumentsCorners[i * 8 + 3] + " " + 
				outDocumentsCorners[i * 8 + 4] + " " + outDocumentsCorners[i * 8 + 5] + " " + 
				outDocumentsCorners[i * 8 + 6] + " " + outDocumentsCorners[i * 8 + 7]);
		}

		Debug.Log("errCode : " + errCode);
	}
}

// Define the functions which can be called from the .dll.
internal static class OpenCVInterop
{
	[DllImport("DocDetectorDLL")]
	internal unsafe static extern int DocumentDetection(uint width, uint height, ref Color32 image, uint maxDocumentsCount, ref uint outDocumentsCount, ref int outDocumentsCorners);

	[DllImport("DocDetectorDLL")]
	internal unsafe static extern int SimpleDocumentDetection(ref Color32 image, uint width, uint height, ref byte result, uint maxDocumentsCount, ref uint outDocumentsCount, ref int outDocumentsCorners);
}
