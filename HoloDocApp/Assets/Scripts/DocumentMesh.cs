using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
public class DocumentMesh : MonoBehaviour
{
	public Material material;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

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
	}
}
