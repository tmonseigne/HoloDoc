using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
public class DocumentMesh : MonoBehaviour
{
	public Material material;
	public Vector3 centroid { get; set; }

	public void CreateDocumentMesh(Vector3[] corners)
	{
		Mesh mesh = new Mesh
		{
			vertices = corners,
			triangles = new int[] 
			{
				0, 1, 2,
				1, 3, 2,
			}
		};

		this.GetComponent<MeshRenderer>().material = material;
		this.GetComponent<MeshFilter>().mesh = mesh;
		this.GetComponent<MeshCollider>().sharedMesh = mesh;

		centroid = Vector3.zero;
		foreach (Vector3 v in corners)
		{
			centroid += v;
		}
		centroid /= corners.Length;
	
		Vector4 centroid4f = new Vector4(centroid.x, centroid.y, centroid.z, 1.0f);
		this.GetComponent<Renderer>().material.SetVector("_Centroid", centroid4f);
	}
}
