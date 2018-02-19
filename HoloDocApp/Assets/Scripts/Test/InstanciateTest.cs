using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanciateTest : MonoBehaviour
{
	public GameObject prefab;

	// Use this for initialization
	void Start()
	{
		GameObject document1 = Instantiate(prefab, this.transform.position, this.transform.rotation);
		Vector3[] corners1 = new Vector3[]
		{
			new Vector3(0, 0, 5),
			new Vector3(0, 0.3f, 5),
			new Vector3(0.3f, 0, 5),
			new Vector3(0.3f, 0.3f, 5),
		};
		document1.GetComponent<DocumentMesh>().CreateDocumentMesh(corners1);
		DocumentProperties prop = document1.GetComponent<DocumentProperties>();
		//Debug.Log(prop.ToString());

		GameObject document2 = Instantiate(prefab, this.transform.position, this.transform.rotation);
		Vector3[] corners2 = new Vector3[]
		{
			new Vector3(0.4f, 0, 5),
			new Vector3(0.4f, 0.3f, 5),
			new Vector3(0.7f, 0, 5),
			new Vector3(0.7f, 0.3f, 5),
		};
		document2.GetComponent<DocumentMesh>().CreateDocumentMesh(corners2);
		prop = document2.GetComponent<DocumentProperties>();
		prop.SetProperties("Exam", "Pierre Benard", "Impossible SIA Exam", "10/01/2018");
		//Debug.Log(prop.ToString());

		GameObject document3 = Instantiate(prefab, this.transform.position, this.transform.rotation);
		Vector3[] corners3 = new Vector3[]
		{
			new Vector3(0, 0.4f, 5),
			new Vector3(0, 0.7f, 5),
			new Vector3(0.3f, 0.4f, 5),
			new Vector3(0.3f, 0.7f, 5),
		};
		document3.GetComponent<DocumentMesh>().CreateDocumentMesh(corners3);
		prop = document3.GetComponent<DocumentProperties>();
		//Debug.Log(prop.ToString());

		GameObject document4 = Instantiate(prefab, this.transform.position, this.transform.rotation);
		Vector3[] corners4 = new Vector3[]
		{
			new Vector3(0.4f, 0.4f, 5),
			new Vector3(0.4f, 0.7f, 5),
			new Vector3(0.7f, 0.4f, 5),
			new Vector3(0.7f, 0.7f, 5),
		};
		document4.GetComponent<DocumentMesh>().CreateDocumentMesh(corners4);
		prop = document4.GetComponent<DocumentProperties>();
		//Debug.Log(prop.ToString());
	}

	// Update is called once per frame
	void Update()
	{
	}
}
