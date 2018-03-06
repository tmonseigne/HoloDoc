using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class EventManager : MonoBehaviour, IInputClickHandler
{
    public void OnPhotoTaken(Texture photo, Matrix4x4 proj, Matrix4x4 world, bool projB, bool worldB)
    {
        Debug.Log(projB);
        Debug.Log(worldB);
        Debug.Log(proj.ToString());
        Debug.Log(world.ToString());

        Debug.Log("Callback");
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("Air tap");
        PhotoCapturer.Instance.TakePhoto(OnPhotoTaken);
    }
}
