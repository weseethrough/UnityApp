using UnityEngine;
using System.Collections;

public class GUICamera : MonoBehaviour {
	
    Vector3 cameraPosition;
    Vector3 startPosition;
    Vector2 draggingStartPos;
    Quaternion startingRotation;
		
	float camera_sensitivity_x = 2.5f;
	float camera_sensitivity_x_hex = 5.5f;
    float camera_sensitivity_y = 2.5f;
	float camera_sensitivity_y_hex = 4.5f;
	
	public float zoomLevel = -1.0f;        

	// Use this for initialization
	void Start () 
    {
        cameraPosition = transform.position;
        startPosition = cameraPosition;
        startingRotation = transform.rotation;
	}
	
	public bool IsPopupDisplayed() 
    {
		HexInfoManager info = GameObject.FindObjectOfType(typeof(HexInfoManager)) as HexInfoManager;
		if(info != null) 
        {
			if(info.IsInOpenStage()) 
            {
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
        dynamicCamPos.x *= IsHexTypeMenu() ? camera_sensitivity_x_hex : camera_sensitivity_x;
	    dynamicCamPos.y *= IsHexTypeMenu() ? camera_sensitivity_y_hex : camera_sensitivity_y;
		
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
			Quaternion rot;
            //provide camera rotation to gui camera. This will elt us roll ui view
            PlayerOrientation p = Platform.Instance.GetPlayerOrientation();
            if(IsHexTypeMenu())
			{
				//slide the camera around
				rot = ConvertOrientation(p, out newCameraOffset);
				//zero rotation
				transform.rotation = startingRotation;
			}
			else
			{
				//Base position on orientation of the main camera
				rot = Camera.main.transform.rotation;

				//zero out pitch and yaw, we only want roll
				Vector3 eulerAngles = rot.eulerAngles;
				newCameraOffset = new Vector2( Mathf.Sin(eulerAngles.x), Mathf.Sin(eulerAngles.y) );
				eulerAngles.x = 0.0f;
				eulerAngles.y = 0.0f;
				rot = Quaternion.Euler(eulerAngles);
				
				transform.rotation = rot;
			}
			
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
		
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            offset = Input.GetTouch(0).deltaPosition;
		
            //makes height distance move to be distance of 3, and proportionally width (eg distance of 5-6)
            offset *= -20.0f / (float)Screen.height;
        }
		else if (Input.multiTouchEnabled) {
			// Don't continue to emulated mouse buttons
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

    public bool IsHexTypeMenu()
    {
        FlowState fs = FlowStateMachine.GetCurrentFlowState();
        while(fs != null)
        {
            if (fs is HexPanel)
            {
                //Debug.Log("Is Hex menu");
                return true;
            }
            fs = fs.parent;
        }
        return false;
    }
}

