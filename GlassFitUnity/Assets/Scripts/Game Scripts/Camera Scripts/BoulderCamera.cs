using UnityEngine;
using System.Collections;

public class BoulderCamera : MonoBehaviour {
	
	GestureHelper.TwoFingerTap twoHandler;
	
	bool started = false;
	
	// Use this for initialization
	void Start () {
		twoHandler = new GestureHelper.TwoFingerTap(() => {
			ResetGyro();
		});
		GestureHelper.onTwoTap += twoHandler;
	}
	
	// Update is called once per frame
	void Update () {
		if(!started)
		{
			Platform.Instance.GetPlayerOrientation().Reset();
			started = true;
		}
		
		PlayerOrientation ori = Platform.Instance.GetPlayerOrientation();
		Quaternion headOffset = Platform.Instance.GetPlayerOrientation().AsQuaternion();
		
		// Double the pitch
		Vector3 eulerAngles = headOffset.eulerAngles;
		eulerAngles.x *= 2.0f;
		//tilt down a little too
		eulerAngles.x -= 15.0f;
		headOffset = Quaternion.Euler(eulerAngles);
		
		// Check for rearview
		Quaternion rearviewOffset = Quaternion.Euler(0, 180, 0);
				
		transform.rotation = rearviewOffset * headOffset;
	}
	
	void ResetGyro()
	{
		Platform.Instance.GetPlayerOrientation().Reset();
	}
}
