using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the train's sounds
/// </summary>
public class TrainSound : MonoBehaviour {
	
	// Time to play whistle.
	private float whistleTime = 0.0f;
	
	// Audio sources for train sounds.
	private AudioSource trainMove;
	private AudioSource trainWhistle;
	
	// Boolean for the train moving sound.
	private bool movePlaying = false;
	
	// Gameobject for the train.
	public GameObject train;
	
	/// <summary>
	/// Gets the sounds from the object
	/// </summary>
	void Start () {
		
		// Get the audio sources for the train sounds.
		var aSources = GetComponents<AudioSource>();
		trainMove = aSources[0];
		trainWhistle = aSources[1];
		
		// If the train is active play the train moving sound
		if(train.activeSelf) {
			trainMove.Play();
			movePlaying = true;
		}
	}
	/// <summary>
	/// Update this instance. Plays the whistle if its time
	/// </summary>
	void Update () {
		// If the train is active.
		if(train.activeSelf) {
			
			// Increase the whistle time.
			whistleTime += Time.deltaTime;
			
			// If the train moving sound isn't playing, play it.
			if(!movePlaying) {
				trainMove.Play();
				movePlaying = true;
			}
			
			// If enough time has passed, play the whistle sound and reset timer.
			if(whistleTime >= 10.0f)
			{
				trainWhistle.Play();
				whistleTime -= 10.0f;
			}
			
			// Set the position of the object based on distance behind object and offset.
			transform.position = new Vector3(0, 0, (float)Platform.Instance.DistanceBehindTarget()-50.0f);
		}
		else {
			// Stop the train move sound and set boolean to false.
			movePlaying = false;
			trainMove.Stop();
		}
	}
}
