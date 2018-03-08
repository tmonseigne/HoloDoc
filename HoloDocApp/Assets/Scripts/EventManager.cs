using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.WSA.Input;

using HoloToolkit.Unity.InputModule;

using TMPro;


public class EventManager : MonoBehaviour, IInputClickHandler
{
    public TextMeshPro message;
    public GameObject colorPreview;

    private CustomFade fadeEffect;
    private Color backgroundColor = Color.blue;
    
    private bool processing = false;
    private bool backgroundPicked = false;
    private int tapCount = 0;
    
    void Start()
    {
        fadeEffect = this.GetComponent<CustomFade>();
    }

    public void OnPhotoTaken(Texture2D photo, Matrix4x4 proj, Matrix4x4 world, bool projB, bool worldB, Resolution resolution)
    {
        Debug.Log(proj);
        Debug.Log(world);
        CustomCameraParameters.ProjectionMatrix = proj;
        CustomCameraParameters.WorldMatrix = world;
        CustomCameraParameters.Resolution = resolution;
        backgroundColor = ColorPicker.Instance.AverageColor(photo);
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (!processing)
        {
            tapCount++;
            if (tapCount == 1)
            {
                Invoke("SingleTap", 0.5f);
            }
            else if (tapCount == 2)
            {
                CancelInvoke("SingleTap");
                DoubleTap();
                tapCount = 0;
            }
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
        backgroundPicked = true;
    }

    public void SingleTap()
    {
        tapCount = 0;
        processing = true;
        PhotoCapturer.Instance.TakePhoto(OnPhotoTaken);
        StartCoroutine(DoFade(backgroundColor));
    }

    public void DoubleTap()
    {
        if (backgroundPicked) {
            SceneManager.LoadScene("SpatialMappingTest");
        }
    }
}
