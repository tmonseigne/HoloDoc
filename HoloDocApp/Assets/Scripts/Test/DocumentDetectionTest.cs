using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class DocumentDetectionTest : MonoBehaviour
{
	[Range(0, 20)] public uint maxDocumentsCount = 5;
	public GameObject quad;
	private Texture2D renderTexture;

	private byte[] result;
	private int[]  outDocumentsCorners;
	private uint[] sizeResolution;

	// Use this for initialization
	void Start()
	{
		sizeResolution = new uint[]
		{
			(uint)CameraStream.Instance.Resolution.width,
			(uint)CameraStream.Instance.Resolution.height
		};

		renderTexture = new Texture2D((int)sizeResolution[0], (int)sizeResolution[1], TextureFormat.RGB24, false);

		outDocumentsCorners = new int[maxDocumentsCount * 8];

		result = renderTexture.GetRawTextureData(); // new byte[frameResolution.width * frameResolution.height * 3];
	}

	// Update is called once per frame
	void Update()
	{
		Color32[] image = CameraStream.Instance.Frame.Data;
		uint outDocumentsCount = 0;

		unsafe
		{
			OpenCVInterop.SimpleDocumentDetection(ref image[0], sizeResolution[0], sizeResolution[1], ref result[0], maxDocumentsCount, ref outDocumentsCount, ref outDocumentsCorners[0]);
		}
		
		renderTexture.LoadRawTextureData(result);
		renderTexture.Apply(true);
		quad.GetComponent<Renderer>().material.mainTexture = renderTexture;
	}
}

// Define the functions which can be called from the .dll.
internal static class OpenCVInterop
{
	[DllImport("DocDetector")]
	internal unsafe static extern int SimpleDocumentDetection(ref Color32 image, uint width, uint height, ref byte result, uint maxDocumentsCount, ref uint outDocumentsCount, ref int outDocumentsCorners);
}
