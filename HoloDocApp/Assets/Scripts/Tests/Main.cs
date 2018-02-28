using UnityEngine;

public class Main : MonoBehaviour {

	[Range(0, 20)]
	public uint nbDocumentsMax = 5;

	public GameObject prefab;

	private Resolution		resolution;
	private byte[]			result;
	private int[]			documentsCorners;
	private GameObject[]	documents;

	// Use this for initialization
	void Start() {
		resolution = CameraStream.Instance.Frame.Resolution;
		result = new byte[resolution.width * resolution.height * 3];
		documentsCorners = new int[nbDocumentsMax * 8];

		documents = new GameObject[nbDocumentsMax];
		for (int i = 0; i < nbDocumentsMax; i++) {
			documents[i] = Instantiate(prefab) as GameObject;
		}
	}

	// Update is called once per frame
	void Update() {
		// Step 1 - We search documents in the frame of the camera.
		Color32[] image = CameraStream.Instance.Frame.Data;
		uint nbDocuments = 0;

		unsafe {
			OpenCVInterop.SimpleDocumentDetection(ref image[0], (uint) resolution.width, (uint) resolution.height,
				ref result[0], nbDocumentsMax, ref nbDocuments, ref documentsCorners[0]);
		}

		// Step 2 - We translate pixel position to 3D position.
		// Then, we create a document mesh which represent the document in the 3D scene.
		int nbCorners = 4;
		Vector3[] corners = new Vector3[nbCorners];
		for (int i = 0; i < nbDocuments; i++) {
			for (int j = 0; j < nbCorners; j++) {
				corners[j] = ConvertPixelTo3DPos(documentsCorners[j * 2], documentsCorners[j * 2 + 1]);
			}

			documents[i].GetComponent<DocumentMesh>().CreateDocumentMesh(corners);
		}
	}

	// pixelX need to be in [0 ; Camera.main.pixelHeight]
	// pixelY need to be in [0 ; Camera.main.pixelWidth]
	Vector3 ConvertPixelTo3DPos(int pixelX, int pixelY) {
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(pixelX, pixelY));

		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100)) {
			return hit.point;
		}

		return new Vector3(10, 10, 10);
	}
}