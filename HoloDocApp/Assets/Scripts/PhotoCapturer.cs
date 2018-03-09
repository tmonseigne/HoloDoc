using System.Linq;

using UnityEngine;
using UnityEngine.XR.WSA.WebCam;

using HoloToolkit.Unity;

public class PhotoCapturer : Singleton<PhotoCapturer> {

    private PhotoCapture    photoCaptureObject;
    private Resolution      cameraResolution;
    private Texture2D       photo;

    [HideInInspector]
    public bool HasFoundCamera;

    public delegate void OnPhotoTakenCallback(Texture2D photo, Matrix4x4 proj, Matrix4x4 world, bool projB, bool worldB, Resolution res);

	// Use this for initialization
	void Start () {
		if (WebCamTexture.devices.Length != 0) {
            //cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
			cameraResolution = new Resolution {
				width = 1280,
				height = 720
			};

			photo = new Texture2D(cameraResolution.width, cameraResolution.height);
			HasFoundCamera = true;
        } 
		else {
            HasFoundCamera = false;
        }
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
			CameraParameters cameraParameters = new CameraParameters {
				hologramOpacity = 0.0f,
				cameraResolutionWidth = cameraResolution.width,
				cameraResolutionHeight = cameraResolution.height,
				pixelFormat = CapturePixelFormat.BGRA32
			};


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
                        callback.Invoke(photo, proj, world, receivedWorld, receivedProj, cameraResolution);
                    }

                    photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
                });
            });
        });
    }
}
