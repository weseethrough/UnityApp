using UnityEngine;
using System.Collections;

//note - current tarmac texture comes from here:
// http://seamless-pixels.blogspot.co.uk/p/blog-page.html

public class virtualTrack : MonoBehaviour {
	
	private const float TrackLength = 1000;
	public float TrackWidth = 4.0f;
	public float scrollFactor = 1.0f;	//world is not 1:1, need this fudge factor
	public bool frozen = false;
	private float Phase = 0;

	// Use this for initialization
	void Start () {
		//plane in scene is 10m x 10m
		
		Vector3 scale = new Vector3(TrackWidth/10.0f, 1.0f, TrackLength/10.0f);
		
		//initialise the dimensions of the track
		transform.localScale = scale;
		
	}
	
	// Update is called once per frame
	void Update () {

		if(!frozen)
		{
			//calculate the UV phase
			float Repeats = renderer.material.mainTextureScale.y;
			Phase = ((float)Platform.Instance.LocalPlayerPosition.Distance * scrollFactor) % (TrackLength/Repeats);
		}
				
		//apply to the material to pass to the shader
		renderer.material.SetFloat("_Phase", Phase);
	}

	
}
