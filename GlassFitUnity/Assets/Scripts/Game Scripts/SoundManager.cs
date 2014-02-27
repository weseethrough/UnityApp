using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {
	
	// Audio sources attached to object
	private static AudioSource tapSound;
	private static AudioSource errorSound;
	private static AudioSource hidePopupSound;
	private static AudioSource showPopupSound;
	private static AudioSource unlockSound;
	
	// Different sound states
	public enum Sounds {
		Tap,
		Error,
		HidePopup,
		ShowPopup,
		Unlock
	};
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake() {
		// Keeps the object alive through scenes
		DontDestroyOnLoad(this);
	}
	
	// Use this for initialization
	void Start () {
		// Array of sounds on GameObject
		AudioSource[] sounds = GetComponents<AudioSource>();
		
		// Sets the AudioSources from the array of sounds above
		if(sounds != null && sounds.Length > 4)
		{
			tapSound = sounds[0];
			errorSound = sounds[1];
			hidePopupSound = sounds[2];
			showPopupSound = sounds[3];
			unlockSound = sounds[4];
		}
	}
	
	/// <summary>
	/// Static function that is used to play the sounds.
	/// </summary>
	/// <param name='sound'>
	/// Sound to play.
	/// </param>/
	public static void PlaySound(Sounds sound)
	{
		switch(sound)
		{
		case Sounds.Tap:
			if(tapSound != null)
			{
				tapSound.Play();
			}
			break;
			
		case Sounds.Error:
			if(errorSound != null)
			{
				errorSound.Play();
			}
			break;
			
		case Sounds.HidePopup:
			if(hidePopupSound != null)
			{
				hidePopupSound.Play();
			}
			break;
			
		case Sounds.ShowPopup:
			if(showPopupSound != null)
			{
				showPopupSound.Play();
			}
			break;
			
		case Sounds.Unlock:
			if(unlockSound != null)
			{
				unlockSound.Play();
			}
			break;
		}
	}
}
