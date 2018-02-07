using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start ()
    { }
	
	// Update is called once per frame
	void Update ()
    {
        Renderer quadRenderer = this.gameObject.GetComponent<Renderer>() as Renderer;
        quadRenderer.material.mainTexture = CameraStream.Instance.Frame;
    }
}
