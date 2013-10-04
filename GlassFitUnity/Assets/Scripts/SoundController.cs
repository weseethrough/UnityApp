using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour {
	private const float NUM_TRACKS = 12;
	private AudioSource[] stevies;
	private float curTime = 0.0f;
	
	private int currentTrack = 1;
	
	// Use this for initialization
	void Start () {
		stevies = GetComponents<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		curTime += Time.deltaTime;
		
		if(curTime >= 10.0f && currentTrack < NUM_TRACKS)
		{
			stevies[currentTrack].volume = 1.0f;
			curTime -= 10.0f;
			currentTrack++;
		}
	}
}
