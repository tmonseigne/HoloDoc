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
        PhotoCapturer.Instance.TakePhoto(onPhotoTaken);
        this.GetComponent<AudioSource>().Play();
    }

    void onPhotoTaken(Texture2D tex)
    {
        Debug.Log("Taaaadaaaaa!");

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Renderer quadRenderer = quad.GetComponent<Renderer>() as Renderer;

        quad.transform.parent = this.transform;
        quad.transform.localPosition = new Vector3(0.0f, 0.0f, 3.0f);

        quadRenderer.material.SetTexture("_MainTex", tex);

        RequestLauncher.Instance.CreateNewDocument(tex);

    }
}
