using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.XR.WSA.WebCam;

public class PhotoCapturer : MonoBehaviour {

    static public PhotoCapturer Instance = null;

    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;
    Resolution cameraResolution;


    public delegate void OnPhotoTakenCallback(Texture2D targetTexture);

    // Use this instead of Start
    // This method is called when the component awake (after Start only if the component is enable)
    private void Awake()
    {
        if (PhotoCapturer.Instance) // If the singleton exists
        {
            DestroyImmediate(this);
        }

        Debug.Log("ici");
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
        PhotoCapturer.Instance = this;

        Debug.Log("la");
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown our photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    public void TakePhoto (OnPhotoTakenCallback callback)
    {
        // Create a PhotoCapture object
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
            photoCaptureObject = captureObject;
            CameraParameters cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            
            // Activate the camera
            photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
                // Take a picture
                photoCaptureObject.TakePhotoAsync((res, pcf) =>
                {
                    pcf.UploadImageDataToTexture(targetTexture);

                    if (callback != null)
                    {
                        callback.Invoke(targetTexture);
                    }

                    photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
                });
            });
        });
    }
}
