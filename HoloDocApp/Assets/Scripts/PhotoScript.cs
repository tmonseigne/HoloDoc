using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoScript : MonoBehaviour {

    public GameObject quad;

    private AudioSource audioSource;
    private AudioClip audioClip;
    private Texture2D renderTexture;

    void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
        audioClip = Resources.Load<AudioClip>("Sounds/Camera_Shutter");
    }

    public void TakePhoto()
    {
        Resolution frameResolution = CameraStream.Instance.Resolution;
        renderTexture = new Texture2D(frameResolution.width, frameResolution.height);

        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
        audioSource.PlayOneShot(audioClip, 1);

        renderTexture.SetPixels32(CameraStream.Instance.Frame.Data);
        renderTexture.Apply(true);

        Renderer quadRenderer = quad.GetComponent<Renderer>() as Renderer;
        quadRenderer.material.mainTexture = renderTexture;
        //RequestLauncher.Instance.CreateNewDocument(tex);
    }
}
