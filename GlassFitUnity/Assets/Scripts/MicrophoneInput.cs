using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour {
	public int volume = 10;
	
    void Start() {
		string AudioInputDevice = Microphone.devices[0];
        audio.clip = Microphone.Start(null, true, 10, 44100);
        audio.loop = true; // Set the AudioClip to loop
        audio.mute = false; // Do not: Mute the sound, we don't want the player to hear it
		audio.volume = volume;
        while (!(Microphone.GetPosition(AudioInputDevice) > 0)){} // Wait until the recording has started
        audio.Play(); // Play the audio source!
    }
}