using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class temp : MonoBehaviour {
	public float width;
	public float height;

	void Start() {
		MeshFilter mf = GetComponent<MeshFilter>();
		mf.mesh = CreateMesh (width, height);
	}


	Mesh CreateMesh(float w, float h)
	{
		Mesh m = new Mesh();
		m.name = "ScriptedMesh";
		m.vertices = new Vector3[] {
			new Vector3(-w, -h, 0),
			new Vector3(w, -h, 0),
			new Vector3(w, h, 0),
			new Vector3(-w, h, 0)
		};
		m.uv = new Vector2[] {
			new Vector2 (0, 0),
			new Vector2 (0, 1),
			new Vector2(1, 1),
			new Vector2 (1, 0)
		};
		m.triangles = new int[] { 0, 1, 2, 0, 2, 3};
		m.RecalculateNormals();

		return m;
	}
}
