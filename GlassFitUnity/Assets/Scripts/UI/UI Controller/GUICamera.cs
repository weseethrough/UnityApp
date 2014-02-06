using UnityEngine;
using System.Collections;

public class GUICamera : MonoBehaviour {
	
    Vector3 cameraPosition;
    Vector3 startPosition;
    Vector2 draggingStartPos;
		
	const float CAMERA_SENSITIVITY_X = 4.5f;
    const float CAMERA_SENSITIVITY_Y = 5.5f;
	
	public float zoomLevel = -1.0f;        

	// Use this for initialization
	void Start () {
        cameraPosition = transform.position;
        startPosition = cameraPosition;
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
		//Vector3 e = p.AsQuaternion().eulerAngles;
		//UnityEngine.Debug.Log("Yaw:" + -p.AsYaw() + ", Pitch:" + -p.AsPitch() + ", Roll:" + -p.AsRoll() + ", x:" + e.x + ", y:" + e.y + ", z:" + e.z);
        //dynamicCamPos *= 0.02f;
        //return Quaternion.EulerRotation(q.eulerAngles.x, 0, q.eulerAngles.z);
        return p.AsQuaternion();
	}
	
	// Update is called once per frame
	/// <summary>
	/// Standard unity function. Moves camera to new location based on different input systems on different platforms
	/// </summary>
	/// <returns></returns>
	void Update () 
    {
        if (!Platform.Instance.IsRemoteDisplay())
        {
            NonSensorNavigation();
        }
        else
        {
            Vector2 newCameraOffset;
            ConvertOrientation(Platform.Instance.GetPlayerOrientation(), out newCameraOffset);
            Vector2 camPos = new Vector2(cameraPosition.x, cameraPosition.y);
            newCameraOffset -= camPos;
            transform.position = new Vector3(newCameraOffset.x, newCameraOffset.y, zoomLevel);
        }
	}

    /// <summary>
    /// 3d hex navigation for touch and mouse devices
    /// </summary>
    /// <returns></returns>
    public void NonSensorNavigation()
    {
        Vector2 offset = Vector2.zero;

        if (Input.touchCount == 1)
        {
            offset = Input.touches[0].deltaPosition;

            //makes height distance move to be distance of 3, and proportionally width (eg distance of 5-6)
            offset *= -20.0f / (float)Screen.height;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            draggingStartPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 newPos = Input.mousePosition;
            offset = newPos - draggingStartPos;
            draggingStartPos = newPos;

            //makes height distance move to be distance of 3, and proportionally width (eg distance of 5-6)
            offset *= 3.0f / (float)Screen.height;
        }        
       
        if (offset.sqrMagnitude != 0)
        {
            Vector3 pos = transform.position;
            pos.x += offset.x;
            pos.y += offset.y;
            transform.position = pos;
        }

    }

    public void ResetCamera()
    {
        transform.position = startPosition;
    }
}

