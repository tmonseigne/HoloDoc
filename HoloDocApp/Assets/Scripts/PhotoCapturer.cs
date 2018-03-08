using System.Linq;

using UnityEngine;
using UnityEngine.XR.WSA.WebCam;

using HoloToolkit.Unity;

public class PhotoCapturer : Singleton<PhotoCapturer> {

    private PhotoCapture photoCaptureObject;
    private Resolution cameraResolution;
    private Texture2D photo;

    public delegate void OnPhotoTakenCallback(Texture2D photo, Matrix4x4 proj, Matrix4x4 world, bool projB, bool worldB);

	// Use this for initialization
	void Start () {
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        photo = new Texture2D(cameraResolution.width, cameraResolution.height);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown our photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    public void TakePhoto(OnPhotoTakenCallback callback)
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
                    pcf.UploadImageDataToTexture(photo);
                    Matrix4x4 world;
                    Matrix4x4 proj;
                    bool receivedWorld = pcf.TryGetCameraToWorldMatrix(out world);
                    bool receivedProj = pcf.TryGetProjectionMatrix(out proj);

                    if (callback != null)
                    {
                        callback.Invoke(photo, proj, world, receivedWorld, receivedProj);
                    }

                    photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
                });
            });
        });
    }
}
