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

        Vector3 centroid3f = Vector3.zero;

        foreach (Vector3 v in corners)
        {
            centroid3f += v;
        }
        centroid3f /= corners.Length;
        Vector4 centroid4f = new Vector4(centroid3f.x, centroid3f.y, centroid3f.z, 1.0f);
        Debug.Log(centroid4f);

        go.GetComponent<DocumentMesh>().CreateDocumentMesh(corners);
        go.GetComponent<DocumentMesh>().material.SetVector("_centroid", centroid4f);

    }
	
	// Update is called once per frame
	void Update () {
	}
}
