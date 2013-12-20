using UnityEngine;
using System.Collections;

//note - current tarmac texture comes from here:
// http://seamless-pixels.blogspot.co.uk/p/blog-page.html

public class virtualTrack : MonoBehaviour {
	
	private const float TrackLength = 3600;
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
		float Phase = -((float)Platform.Instance.Distance() / Repeats) % TrackLength;
				
		//apply to the material to pass to the shader
		renderer.material.SetFloat("_Phase", Phase);
	}
	
	void OnGUI() {
		//show readout of distance run;	
	}
		
}
