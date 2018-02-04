using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoScript : MonoBehaviour {
    public GameObject quad;

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
        Texture2D tex = CameraStream.Instance.Frame;

        Debug.Log("Taaaadaaaaa!");

        Renderer quadRenderer = quad.GetComponent<Renderer>() as Renderer;


        quadRenderer.material.SetTexture("_MainTex", tex);
        //RequestLauncher.Instance.CreateNewDocument(tex);
    }

    void OnPhotoTaken(Texture2D tex)
    {
        

    }
}
