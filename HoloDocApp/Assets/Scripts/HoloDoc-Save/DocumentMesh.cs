using UnityEngine;

[RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
public class DocumentMesh : MonoBehaviour {
    
	public Vector3	Centroid { get; set; }

	public void CreateDocumentMesh(Vector3[] corners) {
		Mesh mesh = new Mesh {
			vertices = corners,
			triangles = new int[] {
				0, 1, 2,
				1, 3, 2,
			}
		};
        
		this.GetComponent<MeshFilter>().mesh = mesh;
		this.GetComponent<MeshCollider>().sharedMesh = mesh;

		Centroid = Vector3.zero;
		foreach (Vector3 v in corners) {
			Centroid += v;
		}

		Centroid /= corners.Length;
	}
}