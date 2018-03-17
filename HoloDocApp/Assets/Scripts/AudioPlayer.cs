using HoloToolkit.Unity;

using UnityEngine;

public class AudioPlayer : Singleton<AudioPlayer> {

	public AudioClip Photo;
	public AudioClip OpenCollection;
	public AudioClip CloseCollection;
	public AudioClip StartLinking;
	public AudioClip EndLinking;
	public AudioClip BreakLinking;
	public AudioClip BadLinking;

	private AudioSource audioPlayer;
	
	void Start () {
		audioPlayer = this.GetComponent<AudioSource>();
	}

	public void PlayClip(AudioClip clip) {
		audioPlayer.PlayOneShot(clip);
	}
}
