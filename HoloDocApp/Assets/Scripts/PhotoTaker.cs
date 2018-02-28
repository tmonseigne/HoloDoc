using System.Collections;

using UnityEngine;

using HoloToolkit.Unity;

[RequireComponent(typeof(AudioSource))]
public class PhotoTaker : Singleton<PhotoTaker> {

	[Tooltip("This is a clip that will be played each time the application will take a photo. This is supposed to be filled with a camera shutter sound sample.")]
	public AudioClip PhotoSound;

	[Range(0, 1)]
	public float photoVolume = 1.0f;

	[Range(0, 5)]
	public float photoDelay = 0.1f;

	private AudioSource audioSource;
	private CameraFrame currentPhoto;

	// Callback is invoked when a photo has been taken
	public delegate void OnPhotoTakenCallback(CameraFrame resultFrame);

	// Use this for initialization
	void Start() {
		audioSource = this.GetComponent<AudioSource>();
		if (PhotoSound == null) {
			PhotoSound = Resources.Load<AudioClip>("Sounds/Camera_Shutter");
		}
	}

	public void Photo(OnPhotoTakenCallback callback) {
		StartCoroutine(TakePhoto(callback));
	}

	IEnumerator TakePhoto(OnPhotoTakenCallback callback) {
		yield return new WaitForSeconds(photoDelay);
		audioSource.PlayOneShot(PhotoSound, photoVolume);
		currentPhoto = CameraStream.Instance.Frame;

		if (callback != null) {
			callback.Invoke(currentPhoto);
		}
	}
}