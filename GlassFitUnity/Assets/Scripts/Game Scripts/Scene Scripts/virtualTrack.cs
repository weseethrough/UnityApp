using UnityEngine;
using System.Collections;

//note - current tarmac texture comes from here:
// http://seamless-pixels.blogspot.co.uk/p/blog-page.html

public class virtualTrack : MonoBehaviour {
	
	private const float TrackLength = 36000;
	private const float TrackWidth = 60;
	private const float scrollFactor = 50.0f;	//world is not 1:1, need this fudge factor
	
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
		float Phase = -((float)Platform.Instance.Distance() * scrollFactor / Repeats) % TrackLength;
				
		//apply to the material to pass to the shader
		renderer.material.SetFloat("_Phase", Phase);
	}

	
}
