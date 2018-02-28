using UnityEngine;

using System.Runtime.InteropServices;

public class DocumentDetectionTest : MonoBehaviour {

	[Range(0, 20)]
	public uint	MaxDocumentsCount = 5;

	public GameObject Quad;

	private int[]		outDocumentsCorners;
	private Texture2D	renderTexture;
	private Resolution	resolution;
	private byte[]		result;

	// Use this for initialization
	private void Start() {
		resolution = CameraStream.Instance.Frame.Resolution;
		renderTexture = new Texture2D(resolution.width, resolution.height, TextureFormat.RGB24, false);
		outDocumentsCorners = new int[MaxDocumentsCount * 8];
		result = renderTexture.GetRawTextureData();
	}

	// Update is called once per frame
	private void Update() {
		var image = CameraStream.Instance.Frame.Data;
		uint outDocumentsCount = 0;

		unsafe
		{
			OpenCVInterop.SimpleDocumentDetection(ref image[0], (uint)resolution.width, (uint)resolution.height, ref result[0],
			MaxDocumentsCount, ref outDocumentsCount, ref outDocumentsCorners[0]);
		}

		renderTexture.LoadRawTextureData(result);
		renderTexture.Apply(true);
		Quad.GetComponent<Renderer>().material.mainTexture = renderTexture;
	}
}

internal static class OpenCVInterop
{
	[DllImport("DocDetector")]
	internal static extern unsafe int SimpleDocumentDetection(ref Color32 image, uint width, uint height, ref byte result,
		uint maxDocumentsCount, ref uint outDocumentsCount, ref int outDocumentsCorners);
}
