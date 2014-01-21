using UnityEngine;
using System.Collections;

public class OnLookAtSound : MonoBehaviour {
	
	public float minPauseBetweenPlays = 5.0f;
	public float lookAngleThresholdDegrees = 30.0f;
	
	protected float timeLastPlayed = 0.0f;
	protected bool bTriggered = false;
	
	public AudioSource audioSource = null;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//test look angle to train object
		Vector3 camForward = Camera.main.transform.forward;
		Vector3 toTrain = gameObject.transform.position - Camera.main.transform.position;
		float angleDegrees = Vector3.Angle(camForward, toTrain);
		
		//check if triggered now
		bool bTriggeredNow = angleDegrees < lookAngleThresholdDegrees;
		
		//if we became triggered, and we're beyond the timeout, trigger the sound
		if(!bTriggered && bTriggeredNow)
		{
			if(timeLastPlayed + minPauseBetweenPlays < Time.time)
			{
				audioSource.Play();
				//record the time last played
				timeLastPlayed = Time.time;
			}
		}
		
		bTriggered = bTriggeredNow;
	}
}
