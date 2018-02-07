using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoScript : MonoBehaviour {

    public GameObject quad;

    private AudioSource m_audioSource;
    private AudioClip m_audioClip;

    public void TakePhoto()
    {
        m_audioSource = this.GetComponent<AudioSource>();
        m_audioClip = Resources.Load<AudioClip>("Sounds/Camera_Shutter");

        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5F);

        m_audioSource.PlayOneShot(m_audioClip, 1);
        
        Texture2D tex = CameraStream.Instance.Frame;

        Renderer quadRenderer = quad.GetComponent<Renderer>() as Renderer;

        quadRenderer.material.SetTexture("_MainTex", tex);
        //RequestLauncher.Instance.CreateNewDocument(tex);
    }

    void OnPhotoTaken(Texture2D tex)
    { }

}
