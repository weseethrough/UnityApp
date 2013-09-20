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
	
	private float scaledPace;
	private float paceSlider;
	private Rect sliderBox;
	private float indoorDistance;
	private float timeChange;
	private bool GoGoGo = false;
	// Use this for initialization
	void Start () {
					#if UNITY_ANDROID && !UNITY_EDITOR 
		inputData = new Platform();
		#else
		inputData = new PlatformDummy();
		#endif
	
	}
	void OnGUI(){
	/*	if(GoGoGo){if(GUI.Button (new Rect(Screen.width/2,Screen.height/2 -250,300,300), "GOGOGO"))
		{
			GoGoGo = true;
		}}
	*/}
	// Update is called once per frame
	void Update () {
		
		inputData.Poll();
		timeChange += Time.deltaTime;
		
		if (timeChange > 10)
		{
			GoGoGo = true;
		} else 
		{
//		inputData
		}
		
		myDistance = (inputData.DistanceBehindTarget())/2f;
		indoorDistance = 20f * Mathf.Sin(timeChange) + 10f * Mathf.Sin(timeChange/2) ;
		Vector3 indoorMove = new Vector3(-10,-14,myDistance);
		
		transform.position =  Vector3.Slerp(transform.position, indoorMove, 0.4f);
		
	if(GoGoGo)
		{
//	float	sacledDistance = inputData.DistanceBehindTarget() * 0.3f;
//	Vector3 movement = new Vector3(0,0,sacledDistance);
//	transform.position = movement;
		}
	}
}
