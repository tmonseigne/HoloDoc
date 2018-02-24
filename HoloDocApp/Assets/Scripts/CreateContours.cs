using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateContours : MonoBehaviour {

	public float thickness = 0.1f;
	public int nbContours = 10;

	// Use this for initialization
	void Start()
	{
		List<Color> colors = CreateColorList(nbContours);
		List<Vector3> corners = new List<Vector3>();
		float boundary = 3f;

		for (int i = 0; i < nbContours; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				float x = Random.Range(-boundary, boundary),
					  y = Random.Range(-boundary, boundary), 
					  z = Random.Range(-boundary, boundary);
				corners.Add(new Vector3(x, y, z));
			}
			CreateContour(corners, thickness, colors[i]);
			corners.Clear();
		}
	}
	
	public void CreateContour(List<Vector3> corners, float thickness, Color color)
	{
		GameObject documentContour = new GameObject { name = "DocumentContour" };

		GameObject documentsCorners = new GameObject { name = "Corners" };
		documentsCorners.transform.parent = documentContour.transform;

		GameObject documentsLinks = new GameObject { name = "Links" };
		documentsLinks.transform.parent = documentContour.transform;

		for (int i = 0; i < corners.Count; i++)
		{
			Vector3 point0 = corners[i], point1 = corners[i == corners.Count - 1 ? 0 : i + 1];

			GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			sphere.transform.position = point0;
			sphere.transform.localScale = new Vector3(thickness, thickness, thickness);
			sphere.GetComponent<Renderer>().material.SetColor("_Color", color);
			sphere.transform.parent = documentsCorners.transform;

			GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			cylinder.transform.position = (point0 + point1) / 2f;
			cylinder.transform.LookAt(point0);
			cylinder.transform.Rotate(Vector3.right, 90);
			cylinder.transform.localScale = new Vector3(thickness, Vector3.Distance(point0, point1) / 2f, thickness);
			cylinder.GetComponent<Renderer>().material.SetColor("_Color", color);
			cylinder.transform.parent = documentsLinks.transform;
		}
	}

	public List<Color> CreateColorList(int nbColors)
	{
		List<Color> colors = new List<Color>();

		for (int i = 0; i < nbColors; i++)
		{
			colors.Add(Color.HSVToRGB(i/(float)nbColors, 1, 1));
		}

		return colors;
	}
}
