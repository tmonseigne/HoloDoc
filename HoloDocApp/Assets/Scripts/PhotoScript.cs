using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoScript : MonoBehaviour {

    public AudioClip shutterSound;
    // Use this for initialization
    public void TakePhoto()
    {
        this.GetComponent<AudioSource>().clip = shutterSound;
        Debug.Log("Photo");
        StartCoroutine(Wait());
        
    }

    IEnumerator Wait() {
        yield return new WaitForSeconds(1);
        this.GetComponent<AudioSource>().Play();
    }
}
