using UnityEngine;
using UnityEngine.UI;

using HoloToolkit.Unity.InputModule;

using TMPro;
using System.Collections;

public class EventManager : MonoBehaviour, IInputClickHandler
{
    public TextMeshPro message;
    public GameObject colorPreview;

    private CustomFade fadeEffect;
    private Color backgroundColor = Color.blue;
    
    void Start()
    {
        fadeEffect = this.GetComponent<CustomFade>();
    }

    public void OnPhotoTaken(Texture2D photo, Matrix4x4 proj, Matrix4x4 world, bool projB, bool worldB)
    {
        Debug.Log(projB);
        Debug.Log(worldB);
        Debug.Log(proj.ToString());
        Debug.Log(world.ToString());

        backgroundColor = ColorPicker.Instance.AverageColor(photo);
    }

    bool processing = false;
    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (!processing)
        {
            processing = true;
            PhotoCapturer.Instance.TakePhoto(OnPhotoTaken);
            StartCoroutine(DoFade(backgroundColor));
        }
    }

    IEnumerator DoFade(Color backgroundColor)
    {
        fadeEffect.Fade(message, 0.0f, 2.5f);
        colorPreview.SetActive(false);
        yield return new WaitForSeconds(1.5f);

        message.text = "This is the color we found. If you wish to redo the operation please double tap, otherwise just air tap to start working.";
        fadeEffect.Fade(message, 1.0f, 2.5f);

        colorPreview.GetComponent<Renderer>().material.color = backgroundColor;
        colorPreview.SetActive(true);
        processing = false;
    }
}
