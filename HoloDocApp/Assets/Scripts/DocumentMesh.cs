using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
public class DocumentMesh : MonoBehaviour
{
	public Material material;

	public void CreateDocumentMesh(Vector3[] corners)
	{
		Mesh mesh = new Mesh();
		Vector3[] vertices = corners;

		int[] faces = new int[]
		{
			0, 1, 2,
			1, 3, 2,
		};

		mesh.vertices = vertices;
		mesh.triangles = faces;

		this.GetComponent<MeshRenderer>().material = material;
		this.GetComponent<MeshFilter>().mesh = mesh;
		this.GetComponent<MeshCollider>().sharedMesh = mesh;

		Vector3 centroid3f = Vector3.zero;
		foreach (Vector3 v in corners)
		{
			centroid3f += v;
		}
		centroid3f /= corners.Length;
		Vector4 centroid4f = new Vector4(centroid3f.x, centroid3f.y, centroid3f.z, 1.0f);
		this.material.SetVector("_centroid", centroid4f);
	}
}
