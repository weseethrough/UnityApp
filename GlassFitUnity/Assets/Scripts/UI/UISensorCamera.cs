using UnityEngine;
using System.Collections;

/// <summary>
/// Used to control the rotation of the camera in the Hex menus
/// </summary>
public class UISensorCamera : MonoBehaviour {
	// Offset for the camera
	public Quaternion offsetFromStart;
	
	// Boolean to initialise the started variable
	private bool started = false;
	private Vector3 scale;
	
	private GestureHelper.OnSwipeLeft backHandler = null;
	
	private GestureHelper.TwoFingerTap twoTapHandler = null;

	private Texture reticleTex = null;

	/// <summary>
	/// Start this instance. Sets the scale for OnGUI
	/// </summary>
	void Start() {
		// Sets the scaling value for the OnGUI 
		scale.x = (float)Screen.width / 800.0f;
		scale.y = (float)Screen.height / 500.0f;
    	scale.z = 1;
		
		twoTapHandler = new GestureHelper.TwoFingerTap(() => {
			Platform.Instance.GetPlayerOrientation().Reset();
		});
		GestureHelper.onTwoTap += twoTapHandler;
		
#if RY_INDOOR
		UnityEngine.Debug.LogError("UISensorCamera: we are indoor, using indoor flow");
		Platform.Instance.LocalPlayerPosition.SetIndoor(true);
		GraphComponent gc = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
    	gc.GoToFlow("IndoorFlow");	
#endif

		reticleTex = Resources.Load("Reticle", typeof(Texture)) as Texture;

		LoadingTextComponent.SetVisibility(false);
	}
	
	void GoBack() 
	{
		FlowState.FollowFlowLinkNamed("MenuButton");
	}
	
	/// <summary>
	/// Raises the GU event. Displays buttons to reset gyros and save
	/// </summary>
	void OnGUI()
	{
		// Set the attributes for the OnGUI elements
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		GUI.skin.button.wordWrap = true;
		GUI.skin.button.fontSize = 15;
		GUI.skin.button.fontStyle = FontStyle.Bold;
		GUI.skin.button.alignment = TextAnchor.MiddleCenter;				
		
		// Reset the Gyro at the start, didn't work in Start()
		if(!started)
		{            
			if (Platform.Instance.OnGlass())
            { 
                Platform.Instance.GetPlayerOrientation().Reset();
            }
            started = true;
		}

#if !RACEYOURSELF_MOBILE		
		//draw a reticle if we're in the hex menu
		if(FindObjectOfType(typeof(DynamicHexList)))
		{
			DrawReticle();
		}
#endif

//		// Delete the save for the training mode
//		if(GUI.Button(new Rect(730, 450, 70, 50), "Reset Save")) {
//			PlayerPrefs.DeleteAll();
//		}
		GUI.matrix = Matrix4x4.identity;
	}
	
	void DrawReticle()
	{
		float width = 800;
		float height = 500;
		float boxHalfSize = 10;
		Rect textureRect = new Rect(width/2.0f - boxHalfSize, height/2.0f - boxHalfSize, 2*boxHalfSize, 2*boxHalfSize);
		GUI.DrawTexture(textureRect, reticleTex, ScaleMode.ScaleToFit, true);
	}
	
	/// <summary>
	/// Update this instance. Sets the rotation of the camera from Platform
	/// </summary>
	void Update () {
		
		Quaternion newOffset = Platform.Instance.GetPlayerOrientation().AsQuaternion();
		
		//UnityEngine.Debug.Log("UISensorCamera: Euler angles are: " + newOffset.eulerAngles.x + ", " + newOffset.eulerAngles.y + ", " + newOffset.eulerAngles.z);
		
		transform.rotation = newOffset;
		
    }
	
	void OnDestroy() {
		GestureHelper.onTwoTap -= twoTapHandler;
		GestureHelper.onSwipeLeft -= backHandler;
	}
}