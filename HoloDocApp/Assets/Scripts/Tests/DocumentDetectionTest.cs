using UnityEngine;

public class DocumentDetectionTest : MonoBehaviour {

	[Range(0, 20)] public uint maxDocumentsCount = 5;
	public GameObject quad;

	private int[] _outDocumentsCorners;
	private Texture2D _renderTexture;
	private Resolution _resolution;

	private byte[] result;

	// Use this for initialization
	private void Start() {
		_resolution = CameraStream.Instance.Frame.Resolution;
		_renderTexture = new Texture2D(_resolution.width, _resolution.height, TextureFormat.RGB24, false);

		_outDocumentsCorners = new int[maxDocumentsCount * 8];

		result = _renderTexture.GetRawTextureData();
	}

	// Update is called once per frame
	private void Update() {
		var image = CameraStream.Instance.Frame.Data;
		uint outDocumentsCount = 0;

		OpenCVInterop.SimpleDocumentDetection(ref image[0], (uint)_resolution.width, (uint)_resolution.height, ref result[0],
			maxDocumentsCount, ref outDocumentsCount, ref _outDocumentsCorners[0]);

		_renderTexture.LoadRawTextureData(result);
		_renderTexture.Apply(true);
		//quad.GetComponent<Renderer>().material.mainTexture = renderTexture;
	}
}
