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
		
		LoadingTextComponent.SetVisibility(false);
	}
	
	void GoBack() 
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		
		GConnector gConect = fs.Outputs.Find(r => r.Name == "MenuButton");
		if(gConect != null) {
			fs.parentMachine.FollowConnection(gConect);
		}
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
#if !UNITY_EDITOR
			Platform.Instance.GetPlayerOrientation().Reset();
#endif
			started = true;
		}

		
		//draw a reticle if we're in the hex menu
		if(FindObjectOfType(typeof(DynamicHexList)))
		{
			DrawReticle();
		}
		
//		// Delete the save for the training mode
//		if(GUI.Button(new Rect(730, 450, 70, 50), "Reset Save")) {
//			PlayerPrefs.DeleteAll();
//		}
		GUI.matrix = Matrix4x4.identity;
	}
	
	void DrawReticle()
	{
		Texture tex = Resources.Load("Reticle", typeof(Texture)) as Texture;
		float width = 800;
		float height = 500;
		float boxHalfSize = 10;
		Rect textureRect = new Rect(width/2.0f - boxHalfSize, height/2.0f - boxHalfSize, 2*boxHalfSize, 2*boxHalfSize);
		GUI.DrawTexture(textureRect, tex, ScaleMode.ScaleToFit, true);
	}
	
	/// <summary>
	/// Update this instance. Sets the rotation of the camera from Platform
	/// </summary>
	void Update () {
		
#if !UNITY_EDITOR
		Quaternion newOffset = Platform.Instance.GetPlayerOrientation().AsQuaternion();
		
		//UnityEngine.Debug.Log("UISensorCamera: Euler angles are: " + newOffset.eulerAngles.x + ", " + newOffset.eulerAngles.y + ", " + newOffset.eulerAngles.z);
		
//		if((newOffset.eulerAngles.x > 30 && newOffset.eulerAngles.x < 330) || (newOffset.eulerAngles.y > 40 && newOffset.eulerAngles.y < 320)) 
//		{
//			DataVault.Set("tutorial_hint", "Tap with two fingers to center view");
//			LoadingTextComponent.SetVisibility(true);
//		}
//		else
//		{
//			string tutHint = (string)DataVault.Get("tutorial_hint");
//			if(tutHint == "Tap with two fingers to center view") {
//				DataVault.Set("tutorial_hint", " ");
//				//LoadingTextComponent.SetVisibility(false);
//			}
//			
//		}
		
		transform.rotation = newOffset;
		
#endif
    }
	
	void OnDestroy() {
		GestureHelper.onTwoTap -= twoTapHandler;
		GestureHelper.onSwipeLeft -= backHandler;
	}
}