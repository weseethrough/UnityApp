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
	
	private GestureHelper.OnSwipeLeft leftHandler = null;
	
	private GestureHelper.OnSwipeRight rightHandler = null;
	
	private float zoomLevel = -1.0f;
	
	// Use this for initialization
	void Start () {
        cameraPosition = transform.position;
        distanceVector = transform.position - cameraPosition;
        distanceVector.y = 0;
        cameraDefaultRotation = transform.rotation;

        radius = distanceVector.magnitude;

#if !UNITY_EDITOR
        Platform.Instance.ResetGyro();
        cameraDefaultRotation = ConvertOrientation(Platform.Instance.GetOrientation(), out cameraMoveOffset);
#endif         
		
		leftHandler = new GestureHelper.OnSwipeLeft(() => {
			if(!IsPopupDisplayed()) {
				if(zoomLevel > -1.5f) {
					zoomLevel -= 0.5f;
				}
			}
		});
		
		GestureHelper.swipeLeft += leftHandler;
		
		rightHandler = new GestureHelper.OnSwipeRight(() => {
			if(!IsPopupDisplayed()) {
				if(zoomLevel < -0.5f) {
					zoomLevel += 0.5f;
				}
			}
		});
		
		GestureHelper.swipeRight += rightHandler;
        
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
	
	public void ResetGyro()
    {
#if !UNITY_EDITOR 
        cameraDefaultRotation = ConvertOrientation(Platform.Instance.GetOrientation(), out cameraMoveOffset);
#endif
    }
	
	void OnDisable() 
	{
		GestureHelper.swipeLeft -= leftHandler;
		GestureHelper.swipeRight -= rightHandler;
	}
	
	void OnDestroy()
	{
		GestureHelper.swipeLeft -= leftHandler;
		GestureHelper.swipeRight -= rightHandler;
	}
	
	/// <summary>
    /// function which converts orientation quaternion into pitch and yaw, suitable for moving cam up/down, left/right on 2D menu
    /// </summary>
    /// <param name="q">input orientation</param>
    /// <param name="dynamicCamPos">(yaw, pitch)</param>
    /// <return>the input quaternion</return>
    private Quaternion ConvertOrientation(Quaternion q, out Vector2 dynamicCamPos)
    {
        float pitch = Mathf.Atan2(2*(q.w*q.x + q.y*q.z), 1-2*(q.x*q.x + q.y*q.y));
        float roll = Mathf.Asin(2*(q.w*q.y - q.z*q.x));
        float yaw = Mathf.Atan2(2*(q.w*q.z + q.x*q.y), 1-2*(q.y*q.y + q.z*q.z));
        
        dynamicCamPos = new Vector2(-yaw, -pitch);
        dynamicCamPos.x *= CAMERA_SENSITIVITY_X;
	    dynamicCamPos.y *= CAMERA_SENSITIVITY_Y;
		
        //UnityEngine.Debug.Log("MenuPosition:" + yaw + ", " + pitch + ", " + roll);
        //dynamicCamPos *= 0.02f;
        //return Quaternion.EulerRotation(q.eulerAngles.x, 0, q.eulerAngles.z);
        return q;
	}
	
	// Update is called once per frame
	void Update () {
		float pitchHeight;
		Vector2 newCameraOffset; 
		ConvertOrientation(Platform.Instance.GetOrientation(), out newCameraOffset);
		Vector3 camPos = transform.position;
		newCameraOffset -= cameraMoveOffset;
        transform.position = new Vector3(newCameraOffset.x, newCameraOffset.y, zoomLevel);
	}
}
