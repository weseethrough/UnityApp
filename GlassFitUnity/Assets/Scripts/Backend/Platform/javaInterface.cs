using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class javaInterface : MonoBehaviour {
	
	/**/
	AndroidJavaClass jc = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper"); 
	/*
void startGPS(){
		
		 jc.CallStatic("startLogging");
	}

void stopGPS(){
		
		 jc.CallStatic("stopLogging");
	}	
	
void pauseGPS(){
		
		 jc.CallStatic("pauseLogging");
	}
	
void syncGPS(){
		
		 jc.CallStatic("syncToLogging");
	}
	*/

void GetPosition(){
		// BG ->>TODO:Get static method for the position data in java side
	//int[] Pos	= jc.Call<int[]>("getCurrentPosition");
		
	//	Debug.Log("position retern" + Pos);
	}	
	
// Use this for initialization
void Start () {
	
			Debug.Log("Scripted started");
		/*
		stopGPS();
		startGPS();
  		pauseGPS();
		syncGPS();
		getPostion();

   float result = jc.CallStatic<float>("getCurrentPace");
	
	 jc.CallStatic("startLogging");
		
		/**/
		
		
	//GUI.Label (new Rect (20, 20, 100, 20), result.ToString());
	//somestring = result.ToString();
		


}

	
       
	
void Update () {
		
		
		

}
}
