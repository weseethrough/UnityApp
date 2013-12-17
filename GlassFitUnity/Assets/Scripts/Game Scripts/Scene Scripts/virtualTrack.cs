using UnityEngine;
using System.Collections;

public class virtualTrack : MonoBehaviour {
	
	private const float TrackLength = 5000;
	private const float TrackWidth = 60;
	
	// Use this for initialization
	void Start () {
		Vector3 scale = new Vector3(TrackWidth, 1.0f, TrackLength);
		//initialise the dimensions of the track
		transform.localScale = scale;
	}
	
	// Update is called once per frame
	void Update () {
		//calculate the UV phase
		
		float Repeats = renderer.material.mainTextureScale.y;
		float Phase = -(Platform.Instance.GetDistance() / Repeats) % TrackLength;
				
		renderer.material.SetFloat("_Phase", Phase);
	}
}
