using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {
	
	private static AudioSource tapSound;
	private static AudioSource errorSound;
	private static AudioSource hidePopupSound;
	private static AudioSource showPopupSound;
	private static AudioSource unlockSound;
	
	public enum Sounds {
		Tap,
		Error,
		HidePopup,
		ShowPopup,
		Unlock
	};
	
	void Awake() {
		DontDestroyOnLoad(this);
	}
	
	// Use this for initialization
	void Start () {
		AudioSource[] sounds = GetComponents<AudioSource>();
		
		if(sounds != null && sounds.Length > 4)
		{
			tapSound = sounds[0];
			errorSound = sounds[1];
			hidePopupSound = sounds[2];
			showPopupSound = sounds[3];
			unlockSound = sounds[4];
		}
	}
	
	// Update is called once per frame
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
