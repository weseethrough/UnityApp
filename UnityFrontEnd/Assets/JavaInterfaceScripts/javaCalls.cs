using UnityEngine;
using System.Collections;


using System.Collections.Generic;

using System.Runtime.InteropServices;

using System;


public class javaCalls : MonoBehaviour 
{
	public float callBackStuff;
	public AndroidJavaClass testCalls;
	public AndroidJavaObject jo ;
	// Use this for initialization
	void Start () 
	{
	
	 testCalls = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper");	
	
		jo = testCalls.CallStatic<AndroidJavaObject>("getHelper");
		
		jo.Call("stopTracking");
		//testCalls.
	}
	
	
	private int originalWidth = 640;  // define here the original resolution
  	private int originalHeight = 400; // you used to create the GUI contents 
 	private Vector3 scale;
	private bool GPSISON = false;
	private string theGPSState = "TURN ON GPS";
	
	void OnGUI(){
	scale.x = Screen.width/originalWidth; // calculate hor scale
    scale.y = Screen.height/originalHeight; // calculate vert scale
    scale.z = 1;
    var svMat = GUI.matrix; // save current matrix
    // substitute matrix - only scale is altered from standard
    GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
	if (GUI.Button (new Rect (10,10,150,100), "get distance")) 
	{
			callBackStuff =  jo.Call<long>("getTargetElapsedDistance");
			//callBackStuff =  testCalls.CallStatic<long>("getTargetElapsedDistance");
			Debug.Log(callBackStuff);
			GUI.Box(new Rect(160,110,150,100), callBackStuff.ToString());
			//	callBackStuff =  jo.Call<float>("getCurrentPace");
			print ("You clicked the button!");
	}

		
	if (GUI.Button (new Rect (10,110,150,100), theGPSState)) 
	{
			
			if(GPSISON)
			{
				jo.Call("stopTracking");
				Debug.Log("Stop the Trackking");
			}
			else
			{
				//testCalls.Call("initGps");
				//testCalls.Call("startTracking");
				jo.Call("initGps");
				jo.Call("startTracking");
	
				
				Debug.Log("Start the Trackking");
			}
	}
	

		
	//if (GUI.Button (new Rect (10,210,150,100), "GPS position")) 
//	{
			
			//testCalls.CallStatic<float>("getCurrentPace");
//			testCalls.Call<float>("getCurrentPace");
//			print ("You clicked the button!");
//	}
		
	  		GUI.matrix = svMat; // restore matrix
	}		
	
	
	
	// Update is called once per frame
		
	void Update () 
	{
		//testCalls myVars = new testCalls();
		if(GPSISON)
		{
				theGPSState = "TURN OFF GPS";
			callBackStuff =  jo.Call<float>("getCurrentPace");
		Debug.Log(callBackStuff);
		//	callBackStuff =  testCalls.CallStatic<float>("getCurrentPace");
		GUI.Box(new Rect(160,110,150,100), callBackStuff.ToString());
		}
		else
		{
				theGPSState = "TURN ON GPS";
		}
		//Debug.Log(callBackStuff.ToString());
	
	}
}
