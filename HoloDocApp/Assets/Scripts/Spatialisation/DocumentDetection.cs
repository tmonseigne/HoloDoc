using UnityEngine;

public class DocumentDetection : MonoBehaviour
{

	[Range(0, 20)] public uint nbDocumentsMax = 5;
	public GameObject prefab;

	private byte[] result;
	private int[] outDocumentsCorners;
	private GameObject[] documents;
	private GameObject[] spheres;

	// Getter DocumentsCorners
	public int[] DocumentsCorners
	{
		get { return this.outDocumentsCorners; }
	}

	// Use this for initialization
	void Start()
	{
		CameraFrame frame = CameraStreamHoloLens.Instance.Frame;
		result = new byte[frame.Resolution.width * frame.Resolution.height * 3];
		outDocumentsCorners = new int[nbDocumentsMax * 8];

		documents = new GameObject[nbDocumentsMax];
		for (int i = 0; i < nbDocumentsMax; i++)
		{
			documents[i] = Instantiate(prefab) as GameObject;
		}

		uint nbSphere = nbDocumentsMax * 4 * 2;
		spheres = new GameObject[nbSphere];
		for (int i = 0; i < nbSphere; i++)
		{
			spheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			spheres[i].transform.position = new Vector3(-10, -10, -10);
			spheres[i].transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
			spheres[i].GetComponent<Collider>().enabled = false;
			spheres[i].GetComponent<Renderer>().material.color = Color.cyan;
			spheres[i].GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Color");
		}
	}

	// Update is called once per frame
	void Update()
	{
		CameraFrame frame = CameraStreamHoloLens.Instance.Frame;
		uint outDocumentsCount = 0;
		
		
		unsafe
		{
			//OpenCVInterop.SimpleDocsDetection(ref frame.Data[0], (uint)frame.Resolution.width, (uint)frame.Resolution.height,
			//	ref result[0], nbDocumentsMax, ref outDocumentsCount, ref outDocumentsCorners[0]);

			Color32 background = PersistentData.WorkspaceBackgroundColor;
			OpenCVInterop.DocsDetection(ref frame.Data[0], (uint) frame.Resolution.width, (uint) frame.Resolution.height,
				background, ref outDocumentsCount, ref outDocumentsCorners[0]);
		}

		int nbCorners = 4;
		Vector3[] corners = new Vector3[nbCorners];
		Vector3? corner = new Vector3?();
		Vector3? corner2 = new Vector3?();
		for (int i = 0; i < outDocumentsCount; i++) {
			for (int j = 0; j < nbCorners; j++) {
				int offset = (i * nbCorners + j) * 2;
				corner = ConvertPixelTo3DPos(new Vector2(outDocumentsCorners[offset], CustomCameraParameters.Resolution.height - outDocumentsCorners[offset + 1]));
				corner2 = ConvertPixelTo3DPos2(new Vector2(outDocumentsCorners[offset], CustomCameraParameters.Resolution.height - outDocumentsCorners[offset + 1]));
				if (corner == null) {
					break;
				}
				Debug.Log("corner : \n" + corner.ToString() + "\n" + corner2.ToString());
				corners[j] = (Vector3)corner;

				spheres[i * nbCorners + j].transform.position = (Vector3)corner;
				spheres[i * nbCorners + j].GetComponent<Renderer>().material.color = Color.blue;

				spheres[nbDocumentsMax * 4 + i * nbCorners + j].transform.position = (Vector3)corner2;
				spheres[nbDocumentsMax * 4 + i * nbCorners + j].GetComponent<Renderer>().material.color = Color.red;

			}

			if (corner != null) {
				documents[i].GetComponent<DocumentMesh>().CreateDocumentMesh(corners);
			}
		}


		/* === IN PROGRESS, DO NOT DELETE IT! ===
		// Method 1
		Vector3[] corners = new Vector3[4] {
			(Vector3) ConvertPixelTo3DPos2(new Vector2(3*CustomCameraParameters.Resolution.width/8, 3*CustomCameraParameters.Resolution.height/8)),
			(Vector3) ConvertPixelTo3DPos2(new Vector2(5*CustomCameraParameters.Resolution.width/8, 3*CustomCameraParameters.Resolution.height/8)),
			(Vector3) ConvertPixelTo3DPos2(new Vector2(5*CustomCameraParameters.Resolution.width/8, 5*CustomCameraParameters.Resolution.height/8)),
			(Vector3) ConvertPixelTo3DPos2(new Vector2(3*CustomCameraParameters.Resolution.width/8, 5*CustomCameraParameters.Resolution.height/8)),
		};

		string strDebug = "Corners1:\n";
		for (int i = 0; i < 4; i++)
		{
			strDebug += i + " " + corners[i].ToString() + "\n";
			spheres[i].transform.position = corners[i];
			spheres[i].GetComponent<Renderer>().material.color = Color.red;
		}

		Debug.Log(strDebug);

		documents[0].GetComponent<DocumentMesh>().CreateDocumentMesh(corners);

		// Method 2
		Vector3[] corners2 = new Vector3[4] {
			(Vector3) ConvertPixelTo3DPos2(new Vector2(                                      0, 0)),
			(Vector3) ConvertPixelTo3DPos2(new Vector2(CustomCameraParameters.Resolution.width, 0)),
			(Vector3) ConvertPixelTo3DPos2(new Vector2(CustomCameraParameters.Resolution.width, CustomCameraParameters.Resolution.height)),
			(Vector3) ConvertPixelTo3DPos2(new Vector2(                                      0, CustomCameraParameters.Resolution.height)),
		};

		string strDebug2 = "Corners2:\n";
		for (int i = 0; i < 4; i++)
		{
			strDebug2 += i + " " + corners2[i].ToString() + "\n";
			spheres[4 + i].transform.position = corners2[i];
			spheres[4 + i].GetComponent<Renderer>().material.color = Color.green;
		}

		Debug.Log(strDebug2);

		documents[1].GetComponent<DocumentMesh>().CreateDocumentMesh(corners2);

		spheres[8].transform.position = (Vector3)ConvertPixelTo3DPos2(new Vector2(CustomCameraParameters.Resolution.width / 2, CustomCameraParameters.Resolution.height / 2));
		spheres[8].GetComponent<Renderer>().material.color = Color.blue;
		*/
	}


	private Vector3? ConvertPixelTo3DPos(Vector2 pixel)
	{
		Vector3 direction = PixelCoordToWorldCoord(CustomCameraParameters.WorldMatrix, CustomCameraParameters.ProjectionMatrix, CustomCameraParameters.Resolution, pixel);

		Vector3? hitPoint = getHitPoint(Camera.main.transform.position, direction);
		Debug.Log("hitPoint V1: " + hitPoint.ToString());

		return hitPoint;
	}



	public static Vector3 PixelCoordToWorldCoord(Matrix4x4 cameraToWorldMatrix, Matrix4x4 projectionMatrix, Resolution cameraResolution, Vector2 pixelCoordinates)
	{
		pixelCoordinates = ConvertPixelCoordsToScaledCoords(pixelCoordinates, cameraResolution); // -1 to 1 coords

		float focalLengthX = projectionMatrix.GetColumn(0).x;
		float focalLengthY = projectionMatrix.GetColumn(1).y;
		float centerX = projectionMatrix.GetColumn(2).x;
		float centerY = projectionMatrix.GetColumn(2).y;

		// On Microsoft Webpage the centers are normalized
		float normFactor = projectionMatrix.GetColumn(2).z;
		centerX = centerX / normFactor;
		centerY = centerY / normFactor;

		Vector3 dirRay = new Vector3((pixelCoordinates.x - centerX) / focalLengthX, (pixelCoordinates.y - centerY) / focalLengthY, 1.0f / normFactor); //Direction is in camera space
		Vector3 direction = new Vector3(Vector3.Dot(cameraToWorldMatrix.GetRow(0), dirRay), Vector3.Dot(cameraToWorldMatrix.GetRow(1), dirRay), Vector3.Dot(cameraToWorldMatrix.GetRow(2), dirRay));

		return direction;
	}
	

	private Vector3? ConvertPixelTo3DPos2(Vector2 pixel)
	{
		Vector2 pixelCoordinates = ConvertPixelCoordsToScaledCoords(pixel, CustomCameraParameters.Resolution); // -1 to 1 coords

		Vector3 CameraSpacePos = UnProjectVector(CustomCameraParameters.ProjectionMatrix, new Vector3(pixelCoordinates.x, pixelCoordinates.y, 1));
		Vector3 cameraLocation = CustomCameraParameters.WorldMatrix.MultiplyPoint(new Vector4(0, 0, 0, 1)); // camera location in world space
		Vector3 rayPoint = CustomCameraParameters.WorldMatrix.MultiplyPoint(CameraSpacePos); // ray point in world space

		Vector3? hitPoint = getHitPoint(cameraLocation, rayPoint - cameraLocation);
		Debug.Log("hitPoint V2: " + hitPoint.ToString());

		return hitPoint;

	}

	private Vector3 UnProjectVector(Matrix4x4 proj, Vector3 to)
	{
		Vector3 from = new Vector3(0, 0, 0);
		var axsX = proj.GetRow(0);
		var axsY = proj.GetRow(1);
		var axsZ = proj.GetRow(2);
		from.z = to.z / axsZ.z;
		from.y = (to.y - (from.z * axsY.z)) / axsY.y;
		from.x = (to.x - (from.z * axsX.z)) / axsX.x;
		return from;
	}

	// **** Launch ray at position to direction and return the hit point ****
	private Vector3? getHitPoint(Vector3 position, Vector3 direction)
	{
		Ray ray = new Ray(position, direction);
		Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);

		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100))
		{
			return hit.point;
		}

		return null;
	}

	// **** Convert pixel coords to scale coords ****
	static Vector2 ConvertPixelCoordsToScaledCoords(Vector2 pixelCoords, Resolution resolution)
	{
		float halfWidth = (float)resolution.width / 2f;
		float halfHeight = (float)resolution.height / 2f;

		//Translate registration to image center;
		pixelCoords.x -= halfWidth;
		pixelCoords.y -= halfHeight;

		//Scale pixel coords to percentage coords (-1 to 1)
		pixelCoords = new Vector2(pixelCoords.x / halfWidth, pixelCoords.y / halfHeight * -1f);

		return pixelCoords;
	}

	static Vector2 ConvertPixelCoordsToScaledCoords2(Vector2 pixelCoords, Resolution resolution)
	{
		Vector2 ImagePosZeroToOne = new Vector2(pixelCoords.x / (float)CustomCameraParameters.Resolution.width, 1.0f - (pixelCoords.y / (float)CustomCameraParameters.Resolution.height));
		pixelCoords = ((ImagePosZeroToOne * 2.0f) - new Vector2(1, 1)); // -1 to 1 space

		return pixelCoords;
	}
}