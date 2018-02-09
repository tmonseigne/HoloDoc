using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PhotoTaker : MonoBehaviour
{
	public static PhotoTaker Instance;

	public AudioClip photoSound;
	[Range(0, 1)]
	public float photoVolume = 1.0f;
	[Range(0, 5)]
	public float photoDelay = 0.1f;

	private AudioSource audioSource;
	private CameraFrame currentPhoto;

	// Callback is invoked when a photo has been taken
	public delegate void OnPhotoTakenCallback(CameraFrame resultFrame);

	// Use this for initialization
	void Start()
	{
		if (Instance)
		{
			DestroyImmediate(this);
		}
		audioSource = this.GetComponent<AudioSource>();
		if (photoSound == null)
		{
			photoSound = Resources.Load<AudioClip>("Sounds/Camera_Shutter");
		}
		Instance = this;
	}

	public void Photo(OnPhotoTakenCallback callback)
	{
		StartCoroutine(TakePhoto(callback));
	}

	IEnumerator TakePhoto(OnPhotoTakenCallback callback)
	{
		yield return new WaitForSeconds(photoDelay);
		audioSource.PlayOneShot(photoSound, photoVolume);
		currentPhoto = CameraStream.Instance.Frame;

		if (callback != null)
		{
			callback.Invoke(currentPhoto);
		}
	}
}
