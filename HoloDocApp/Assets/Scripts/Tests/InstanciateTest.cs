using UnityEngine;

public class InstanciateTest : MonoBehaviour {

	[Range(1, 20)] public int nbLines = 2;
	[Range(1, 20)] public int nbColumn = 2;
	public GameObject prefab;

	// Use this for initialization
	void Start() {
		float offset = 0.4f;

		for (int i = 0; i < nbLines; i++) {
			float offsetI = i * offset;
			for (int j = 0; j < nbColumn; j++) {
				float offsetJ = j * offset;

				GameObject document = Instantiate(prefab, this.transform.position, this.transform.rotation);
				Vector3[] corners = new Vector3[] {
					new Vector3(0 + offsetI, 0 + offsetJ, 5),
					new Vector3(0 + offsetI, 0.3f + offsetJ, 5),
					new Vector3(0.3f + offsetI, 0 + offsetJ, 5),
					new Vector3(0.3f + offsetI, 0.3f + offsetJ, 5)
				};
				document.GetComponent<DocumentMesh>().CreateDocumentMesh(corners);
				//DocumentProperties DocProperties = document.GetComponent<DocumentProperties>();
			}
		}
	}

	// Update is called once per frame
	void Update() {}
}