using UnityEngine;
using System.Runtime.InteropServices;

public class DocumentDetectionTest : MonoBehaviour
{
	[Range(0, 20)] public uint maxDocumentsCount = 5;
	public GameObject quad;
	private Texture2D renderTexture;

	private byte[] result;
	private int[]  outDocumentsCorners;
	private Resolution resolution;

	// Use this for initialization
	void Start()
	{
		resolution = CameraStream.Instance.Frame.Resolution;
		renderTexture = new Texture2D(resolution.width, resolution.height, TextureFormat.RGB24, false);

		outDocumentsCorners = new int[maxDocumentsCount * 8];

		result = renderTexture.GetRawTextureData(); 
	}

	// Update is called once per frame
	void Update()
	{
		Color32[] image = CameraStream.Instance.Frame.Data;
		uint outDocumentsCount = 0;

		unsafe
		{
			OpenCVInterop.SimpleDocumentDetection(ref image[0], (uint)resolution.width, (uint)resolution.height, ref result[0], maxDocumentsCount, ref outDocumentsCount, ref outDocumentsCorners[0]);
		}
		
		renderTexture.LoadRawTextureData(result);
		renderTexture.Apply(true);
		//quad.GetComponent<Renderer>().material.mainTexture = renderTexture;
	}
}

/*
// Define the functions which can be called from the .dll.
internal static class OpenCVInterop
{
	[DllImport("DocDetector")]
	internal unsafe static extern int SimpleDocumentDetection(ref Color32 image, uint width, uint height, ref byte result, uint maxDocumentsCount, ref uint outDocumentsCount, ref int outDocumentsCorners);
}*/
