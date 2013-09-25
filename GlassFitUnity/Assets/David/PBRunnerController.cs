using UnityEngine;
using System.Collections;

public class PBRunnerController : MonoBehaviour {
	
	#if UNITY_ANDROID && !UNITY_EDITOR 
	private Platform inputData = null;
#else
	private PlatformDummy inputData = null;
#endif
	
	private Transform CurrentLocation;
	private Transform targetLocation;
	private float myDistance;
	private float targetDistance;
	
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
		#if UNITY_ANDROID && !UNITY_EDITOR 
		inputData = new Platform();
		#else
		inputData = new PlatformDummy();
		#endif
	
		inputData.Start(false);
	}
	
	void OnGUI() 
	{
		GUI.Label(new Rect(Screen.width/2, Screen.height/2, 300, 300), scaledDistance.ToString());	
	}
	
	void Update () {
		
		inputData.Poll();
		//timeChange += Time.deltaTime;
		
//		if (timeChange > 10)
//		{
//			GoGoGo = true;
//		} else 
//		{
//
//		}
		
		if(!started && Input.touchCount == 3)
		{
			started = true;
			inputData.Start(false);
		}
//		
//		myDistance = (inputData.DistanceBehindTarget());
//		Vector3 indoorMove = new Vector3(-10,-14,myDistance);
//		
//		transform.position =  Vector3.Slerp(transform.position, indoorMove, 0.4f);

//		if(GoGoGo)
//		{
			scaledDistance = inputData.DistanceBehindTarget() * 6.666f;
			Vector3 movement = new Vector3(-10,-14,(float)scaledDistance);
			transform.position = movement;
		//}
	}
}
