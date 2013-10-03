using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class ZombieController : MonoBehaviour {

	private Platform inputData = null;
	
	public GameObject platform;
	private Transform CurrentLocation;
	private Transform targetLocation;
	private float myDistance;
	private float targetDistance;
	public bool indoor = false;
	private float FakedMovement = -250f;
	
	private float countTime = 3.99f;
	private bool started = false;
	
	private float scaledPace;
	private float paceSlider;
	private Rect sliderBox;
	private float indoorDistance;
	private float timeChange;
	private double scaledDistance;
	
	private Rect target;	
	private string targetText;
	
	private Vector3 scale;
	private int originalWidth = 800;
	private int originalHeight = 500;
	
	// Background textures
	Texture2D normal;
	Texture2D info;
	Texture2D warning;
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
		target =   new Rect(15, 15, 200, 100);
		
		scale.x = (float)Screen.width / originalWidth;
		scale.y = (float)Screen.height / originalHeight;
    	scale.z = 1;
		
		Color white = new Color(0.9f, 0.9f, 0.9f, 0.5f);
		normal = new Texture2D(1, 1);
		normal.SetPixel(0,0,white);
		normal.Apply();
		
		Color green = new Color(0f, 0.9f, 0f, 0.5f);
		info = new Texture2D(1, 1);
		info.SetPixel(0,0,green);
		info.Apply();
		
		Color red = new Color(0.9f, 0f, 0f, 0.5f);
		warning = new Texture2D(1, 1);
		warning.SetPixel(0,0,red);
		warning.Apply();
	}
	
	void OnEnable() {
		transform.position = new Vector3(-10, -81.7f, 0);
		//inputData = new Platform(); 
	}
		
	void OnGUI() {
		
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
				
		GUI.skin.box.wordWrap = true;
		GUI.skin.box.fontSize = 30;
		GUI.skin.box.fontStyle = FontStyle.Bold;
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;				
		GUI.skin.box.normal.background = normal;
		GUI.skin.box.normal.textColor = Color.black;
		
		// Target Distance
		GUIStyle targetStyle = new GUIStyle(GUI.skin.box);
		double targetDistance = inputData.DistanceBehindTarget()-20;
		if (targetDistance > 0) {
			targetStyle.normal.background = warning; 
			targetText = "Behind!\n";
		} else {
			targetStyle.normal.background = info; 
			targetText = "Ahead\n";
		}
		targetStyle.normal.textColor = Color.white;		
		GUI.Box(target, targetText+"<i>"+SiDistance( Math.Abs(targetDistance) )+"</i>", targetStyle);
	}
	
	void Update () {
		
//		if(countTime == 3.99f && inputData.hasLock() && !started)
//		{
//			started = true;
//		}
//		
//		if(started && countTime <= 0.0f)
//		{
//			inputData.StartTrack(false);
//		}
//		else if(started && countTime > 0.0f)
//		{
//			countTime -= Time.deltaTime;
//		}
		
		inputData.Poll();
		
//		if(!started && Input.touchCount == 3)
//		{
//			started = true;
//			inputData.Start(false);
//		}
		
		//FakedMovement  += (Time.deltaTime * 3)*6;
		
//		if(!indoor) 
//		{
		

		
		//		}
//		else 
//		{
//			FakedMovement  += (Time.deltaTime * 3)*6;
//			scaledDistance = FakedMovement;
//		}
		
		
		scaledDistance = (inputData.DistanceBehindTarget()-20) * 76.666f;
		//scaledDistance = FakedMovement;
		Vector3 movement = new Vector3(-10, -81.7f,(float)scaledDistance);
		transform.position = movement;
	}
	
	public void reset()
	{
		scaledDistance = 0;
	}
	
	string SiDistance(double meters) {
		string postfix = "m";
		string final;
		int value = (int)meters;
		if (value > 1000) {
			value = value/1000;
			postfix = "km";
			final = value.ToString("f3");
		}
		else
		{
			final = value.ToString("f0");
		}
		return final+postfix;
	}
}
