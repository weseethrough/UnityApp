using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

using System.Diagnostics;
using System;


public class SlowRunnerController : MonoBehaviour 
{
	
	#if UNITY_ANDROID && !UNITY_EDITOR 
	private Platform inputData = null;
#else
	private PlatformDummy inputData = null;
#endif
	
	private Transform CurrentLocation;
	private Transform targetLocation;
	private bool GoGoGo = false;
	private float	scaledPace;
	private float paceSlider;
	private Rect sliderBox;
	private float indoorDistance;
	private float timeChange;
	
	private Stopwatch lerpTimer = new Stopwatch();
	// Use this for initialization
	void Start () {
		#if UNITY_ANDROID && !UNITY_EDITOR 
		inputData = new Platform();
		#else
		inputData = new PlatformDummy();
		#endif
		sliderBox = new Rect(Screen.width/2, 15, 500,200); 
		
	}
	void OnGUI(){

		GUIStyle theBox = new GUIStyle(GUI.skin.horizontalSlider);
		
		GUIStyle sliderStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
		
		sliderStyle.fixedHeight = 60;
		
		sliderStyle.fixedWidth  = 60;
		
		paceSlider = GUI.HorizontalSlider(sliderBox,paceSlider,-10,25,theBox,sliderStyle);
		/*if(!GoGoGo){
		if(GUI.Button (new Rect(Screen.width/2-550,Screen.height/2 -250,300,300), "GOGOGO"))
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
		}
		
		float myPace = 1;
		float tempPace = 0;
	
		if(GoGoGo)
		{
			tempPace = inputData.Pace();
			if( tempPace>0.1 && tempPace < 30)
			{
				myPace = (1/tempPace)/60 * 1000;
			}
			else{
				myPace = 10f;
			}
			
		scaledPace = (float)((((myPace)- paceSlider)) * Time.deltaTime)*0.2f;
			
		transform.position = new Vector3(10, -14, transform.position.z);
		Vector3 movement = new Vector3(0,0,scaledPace);
		transform.position += movement;
		
				
		}
	}
}
