using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class javaInterface : MonoBehaviour {

	
private string somestring = "foooooooooooOOooo";
// Use this for initialization
void Start () {
// you can use the API directly:
Sensor.Activate(Sensor.Type.RotationVector);

		AndroidJavaClass jc = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper"); 

   float result = jc.Call<float>("getCurrentPace");
	GUI.Label (new Rect (20, 20, 100, 20), result.ToString());
	somestring = result.ToString();
// or you can use the SensorHelper, which has built-in fallback to less accurate but more common sensors:
//	SensorHelper.ActivateRotation();
SensorHelper.TryForceRotationFallback(RotationFallbackType.RotationQuaternion);
//	SensorHelper.TryForceRotationFallback(RotationFallbackType.OrientationAndAcceleration);
useGUILayout = false;
}
//	void OnGUI () {
       


 //       GUI.Label (new Rect (20, 20, 100, 20), somestring);
   // }
//public javaInterface testing = new javaInterface ();
// Update is called once per frame
void Update () {
//testing.Share();
// direct Sensor usage:
transform.rotation = Sensor.rotationQuaternion;// --- is the same as Sensor.QuaternionFromRotationVector(Sensor.rotationVector);
// Helper with fallback:
//transform.rotation = SensorHelper.rotation;
}
}
