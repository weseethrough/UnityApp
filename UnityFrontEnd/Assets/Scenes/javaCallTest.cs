using UnityEngine;

using System.Collections;

using System.Runtime.InteropServices;

using System;



public class javaCallTest: MonoBehaviour {


private string somestring = "foooooooooooOOooo";

AndroidJavaClass jc;

AndroidJavaClass jcPos;

AndroidJavaObject ji;
	
public static void callBack(string test){
		Debug.Log(test);
	}

// Use this for initialization
	
void Start () {

Debug.Log("START");

//jcPos = new AndroidJavaClass("com.glassfitgames.glassfitplatform.models.Position");

jc = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper");

//ji = jc.CallStatic<AndroidJavaObject>("currentActivity");


useGUILayout = false;

}



//void Update () {

void OnGUI () {


Debug.Log("hello7");


string result2 = jc.CallStatic<string>("myName");

Debug.Log("result: " + result2);

}

}

