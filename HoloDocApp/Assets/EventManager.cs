using UnityEngine;
using UnityEngine.UI;

using HoloToolkit.Unity.InputModule;

using TMPro;
using System.Collections;

public class EventManager : MonoBehaviour, IInputClickHandler
{
    public TextMeshProUGUI message;
    public GameObject rawImage;

    public void OnPhotoTaken(Texture2D photo, Matrix4x4 proj, Matrix4x4 world, bool projB, bool worldB)
    {
        Debug.Log(projB);
        Debug.Log(worldB);
        Debug.Log(proj.ToString());
        Debug.Log(world.ToString());

        Color c = ColorPicker.Instance.AverageColor(photo);
		Debug.Log(c);

		StartCoroutine(DoFade(c));
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("Air tap");
        PhotoCapturer.Instance.TakePhoto(OnPhotoTaken);
		//StartCoroutine(DoFade(Color.green));
    }

    IEnumerator DoFade(Color backgroundColor)
    {
        message.CrossFadeAlpha(0.0f, 1.0f, false);
        yield return new WaitForSeconds(1.1f);

        message.text = "This is the color we found. If you wish to redo the operation please double tap, otherwise just air tap to start working.";
        message.CrossFadeAlpha(1.0f, 1.0f, false);

        rawImage.GetComponent<RawImage>().color = backgroundColor;
        rawImage.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        rawImage.SetActive(true);
        rawImage.GetComponent<RawImage>().CrossFadeAlpha(1.0f, 1.5f, false);
    }
}
