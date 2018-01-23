using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateCube : MonoBehaviour {

    Vector3 axis;
    bool switched = true;

    // Use this for initialization
	void Start () {
        this.axis = Vector3.up;
    }
	
	// Update is called once per frame
	void Update () {
        this.transform.Rotate(this.axis, 2f);
    }

    public void switchAxis()
    {
        Material mat = this.GetComponent<Renderer>().sharedMaterial;
        if (switched)
        {
            mat.SetColor("_Color", Color.red);
        }
        else
        {
            mat.SetColor("_Color", Color.blue);
        }

        this.switched = !this.switched;
    }
}
