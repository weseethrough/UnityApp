using UnityEngine;
using System.Collections;

public class PresetTrackController : MonoBehaviour {

	
	private float countTime = 3.99f;
	private bool started = false;
	
	private float scaledPace;
	private float paceSlider;
	private Rect sliderBox;
	private float indoorDistance;
	private float timeChange;
	private double scaledDistance;
	private bool GoGoGo = false;
	
	// Use this for initialization
	void Start () {
		//Platform.Instance.setTargetTrack(0);
	}
	
	void OnEnable() {
		transform.position = new Vector3(10, -10, 0);
	}
	
	void Update () {
		
//		if(Input.touchCount == 2 && !started)
//		{
//			started = true;
//		}
//		
//		if(started && countTime <= 0.0f)
//		{
//			Platform.Instance.StartTrack(false);
//		}
//		else if(started && countTime > 0.0f)
//		{
//			countTime -= Time.deltaTime;
//		}
		
		Platform.Instance.Poll();
		
//		if(!started && Input.touchCount == 3)
//		{
//			started = true;
//			Platform.Instance.Start(false);
//		}

		scaledDistance = Platform.Instance.DistanceBehindTarget() * 6.666f;
		Vector3 movement = new Vector3(-10,-14,(float)scaledDistance);
		transform.position = movement;
	}
}
