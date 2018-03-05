using UnityEngine;

public class DocumentDetection : MonoBehaviour {

	[Range(0, 20)] public uint nbDocumentsMax = 5;
	public GameObject prefab;

	private byte[] result;
	private int[] outDocumentsCorners;
	private GameObject[] documents;

	// Getter DocumentsCorners
	public int[] DocumentsCorners {
		get { return this.outDocumentsCorners; }
	}

	// Use this for initialization
	void Start() {
		CameraFrame frame = CameraStreamHoloLens.Instance.Frame;
		result = new byte[frame.Resolution.width * frame.Resolution.height * 3];
		outDocumentsCorners = new int[nbDocumentsMax * 8];

		documents = new GameObject[nbDocumentsMax];
		for (int i = 0; i < nbDocumentsMax; i++) {
			documents[i] = Instantiate(prefab) as GameObject;
		}
	}

	// Update is called once per frame
	void Update()
	{
		CameraFrame frame = CameraStreamHoloLens.Instance.Frame;
		uint outDocumentsCount = 0;

		unsafe
		{
			OpenCVInterop.SimpleDocsDetection(ref frame.Data[0], (uint)frame.Resolution.width, (uint)frame.Resolution.height, 
				ref result[0], nbDocumentsMax, ref outDocumentsCount, ref outDocumentsCorners[0]);
			/*
			Color32 background = new Color32(0,0,0,1);
			OpenCVInterop.DocsDetection(ref frame.Data[0], (uint) frame.Resolution.width, (uint) frame.Resolution.height, 
				background, ref outDocumentsCount, ref outDocumentsCorners[0]);
			*/
		}

		// /!\ TODO: Need to check that the size of the HoloLens camera is the same as the size of the camera on Unity
		int nbCorners = 4;
		Vector3[] corners = new Vector3[nbCorners];
		Vector3? corner = new Vector3?();
		for (int i = 0; i < outDocumentsCount; i++) {
			for (int j = 0; j < nbCorners; j++) {
				int offset = (i * nbCorners + j) * 2;
				corner = ConvertPixelTo3DPos(outDocumentsCorners[offset], Camera.main.pixelHeight - outDocumentsCorners[offset + 1]);
				if (corner == null) {
					break;
				}
				corners[j] = (Vector3)corner;
			}
			
			if (corner != null) {
				documents[i].GetComponent<DocumentMesh>().CreateDocumentMesh(corners);
			}
		}
	}

	// pixelX need to be in [0 ; Camera.main.pixelWidth]
	// pixelY need to be in [0 ; Camera.main.pixelHeight]
	private Vector3? ConvertPixelTo3DPos(int pixelX, int pixelY) {
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(pixelX, pixelY));
		Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);

		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100)) {
			return hit.point;
		}

		return null;
	}
}
