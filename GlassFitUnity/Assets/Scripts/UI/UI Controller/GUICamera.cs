using UnityEngine;
using System.Collections;

public class GUICamera : MonoBehaviour {
	
	Vector3 distanceVector;
    Vector3 cameraPosition;
    float radius;
	
	//default height and rotation is used as a default offset when reseting sensors
    Quaternion cameraDefaultRotation;
    Vector2 cameraMoveOffset;
	
	const float CAMERA_SENSITIVITY_X = 4.5f;
    const float CAMERA_SENSITIVITY_Y = 5.5f;
	
	public float zoomLevel = -1.0f;
	
	// Use this for initialization
	void Start () {
        cameraPosition = transform.position;
        distanceVector = transform.position - cameraPosition;
        distanceVector.y = 0;
        cameraDefaultRotation = transform.rotation;

        radius = distanceVector.magnitude;

#if !UNITY_EDITOR
        Platform.Instance.ResetGyro();
        cameraDefaultRotation = ConvertOrientation(Platform.Instance.GetPlayerOrientation(), out cameraMoveOffset);
#endif         
		
        
	}
	
	public bool IsPopupDisplayed() {
		HexInfoManager info = GameObject.FindObjectOfType(typeof(HexInfoManager)) as HexInfoManager;
		if(info != null) {
			if(info.IsInOpenStage()) {
				info.AnimExit();
				return true;
			}
		}
		return false;
	}	
	
	/// <summary>
    /// function which converts orientation quaternion into pitch and yaw, suitable for moving cam up/down, left/right on 2D menu
    /// </summary>
    /// <param name="q">input orientation</param>
    /// <param name="dynamicCamPos">(yaw, pitch)</param>
    /// <return>the input quaternion</return>
    private Quaternion ConvertOrientation(PlayerOrientation p, out Vector2 dynamicCamPos)
    {
        
        dynamicCamPos = new Vector2(p.AsCumulativeYaw(), -p.AsPitch());
        dynamicCamPos.x *= CAMERA_SENSITIVITY_X;
	    dynamicCamPos.y *= CAMERA_SENSITIVITY_Y;
		
        //UnityEngine.Debug.Log("Yaw:" + -p.AsYaw() + ", x-offset:" + dynamicCamPos.x);
		Vector3 e = p.AsQuaternion().eulerAngles;
		UnityEngine.Debug.Log("Yaw:" + -p.AsYaw() + ", Pitch:" + -p.AsPitch() + ", Roll:" + -p.AsRoll() + ", x:" + e.x + ", y:" + e.y + ", z:" + e.z);
        //dynamicCamPos *= 0.02f;
        //return Quaternion.EulerRotation(q.eulerAngles.x, 0, q.eulerAngles.z);
        return p.AsQuaternion();
	}
	
	// Update is called once per frame
	void Update () {
		float pitchHeight;
		Vector2 newCameraOffset; 
		ConvertOrientation(Platform.Instance.GetPlayerOrientation(), out newCameraOffset);
		Vector3 camPos = transform.position;
		newCameraOffset -= cameraMoveOffset;
        transform.position = new Vector3(newCameraOffset.x, newCameraOffset.y, zoomLevel);
	}
}
