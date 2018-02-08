using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInstanciate : MonoBehaviour {

    public GameObject prefab;
	// Use this for initialization
	void Start () {
        GameObject go = Instantiate(prefab, this.transform.position, this.transform.rotation);
        Vector3[] corners = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
        };
        go.GetComponent<DocumentMesh>().CreateDocumentMesh(corners);
	}
	
	// Update is called once per frame
	void Update () {
	}
}
