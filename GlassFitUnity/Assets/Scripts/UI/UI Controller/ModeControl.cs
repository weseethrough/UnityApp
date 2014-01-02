using UnityEngine;
using System.Collections;
using System;

public class ModeControl : MonoBehaviour {

	private UIAtlas atlas;
	private UISprite sprite;
//	
//	private bool rearview = false;
//	
//	private bool indoor = false;
//	
//	private bool camera = false;
	
	private bool setting = false;
	
	GestureHelper.OnTap tapHandler = null;
	
	GestureHelper.DownSwipe downHandler = null;
	
	private string currentMode = "";
	
	// Use this for initialization
	void Start () {
		GameObject flow = GameObject.Find("Flow");
		UnityEngine.Debug.Log("Mode: flow found");
		
		atlas = flow.GetComponent<GraphComponent>().m_defaultHexagonalAtlas;
		UnityEngine.Debug.Log("Mode: atlas found");
		
		sprite = GetComponentInChildren<UISprite>();
		UnityEngine.Debug.Log("Mode: sprite found");
		
		if(atlas != null)
		{
			if(sprite != null) {
				UnityEngine.Debug.Log("Mode: everything found");
				sprite.atlas = atlas;
				UnityEngine.Debug.Log("Mode: image is called " + (string)DataVault.Get ("image_name"));
				sprite.spriteName = (string)DataVault.Get ("image_name");
				UnityEngine.Debug.Log("Mode: sprite set");
			}
			else 
			{
				UnityEngine.Debug.Log("Mode: problem with sprite");
			}
		}
		else
		{
			UnityEngine.Debug.Log("Mode: problem with atlas");
		}
		
		currentMode = (string)DataVault.Get("game_name");
		
		currentMode = currentMode.Replace(" ", "_");
		
		UnityEngine.Debug.Log("Mode: Name is: " + currentMode.ToLower());
		
		setting = Convert.ToBoolean(DataVault.Get(currentMode.ToLower()));
		
//		if(currentMode == "Rear-view mirror") 
//		{
//			rearview = (bool)DataVault.Get("rearview");
//			UnityEngine.Debug.Log("Mode: rearview is set to: " + rearview.ToString());
//			if(rearview) {
//				DataVault.Set("active_mode", "Tap to turn off");
//				rearview = true;
//			} else {
//				DataVault.Set("active_mode", "Tap to turn on");
//				rearview = false;
//			}
//			
//		} else if(currentMode == "Settings")
//		{
//			indoor = (bool)DataVault.Get("indoor");
//			UnityEngine.Debug.Log("Mode: indoor is set to: " + indoor.ToString());
//			if(indoor) {
//				DataVault.Set("active_mode", "Tap to turn off");
//				indoor = true;
//			} else {
//				DataVault.Set("active_mode", "Tap to turn on");
//				indoor = false;
//			}
//		} else if(currentMode == "Camera")
//		{
//			camera = (bool)DataVault.Get("camera");
//			UnityEngine.Debug.Log("Mode: camera is set to: " + camera.ToString());
//			if(camera)
//			{
//				DataVault.Set("active_mode", "Tap to turn off");
//			} else {
//				DataVault.Set("active_mode", "Tap to turn on");
//				camera = false;
//			}
//		}
		
		if(setting) {
			DataVault.Set("active_mode", "Tap to turn off");
		} else {
			DataVault.Set("active_mode", "Tap to turn on");
		}
			
		tapHandler = new GestureHelper.OnTap(() => {
			SetMode();
		});
		GestureHelper.onTap += tapHandler;
			
		downHandler = new GestureHelper.DownSwipe(() => {
			BackToMenu();
		});
		
		GestureHelper.onSwipeDown += downHandler;
			
	}
	
	void BackToMenu() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "MenuButton");
		if(gConnect != null) {
			fs.parentMachine.FollowConnection(gConnect);
		} else {
			UnityEngine.Debug.Log("Mode: Menu button not found!!");
		}
	}
	
	void SetMode() {
		UnityEngine.Debug.Log("Mode: screen tapped");
//		if(currentMode == "Rearview") 
//		{
//			if(rearview) {
//				DataVault.Set("active_mode", "Tap to turn on");
//				rearview = false;
//				DataVault.Set("rearview", rearview);
//			} else {
//				DataVault.Set("active_mode", "Tap to turn off");
//				rearview = true;
//				DataVault.Set("rearview", rearview);
//			}
//		} else if(currentMode == "Settings") 
//		{
//			if(indoor) {
//				DataVault.Set("active_mode", "Tap to turn on");
//				indoor = false;
//				DataVault.Set("indoor", indoor);
//			} else 
//			{
//				DataVault.Set("active_mode", "Tap to turn off");
//				indoor = true;
//				DataVault.Set("indoor", indoor);
//			}
//		}
		if(setting) {
			DataVault.Set("active_mode", "Tap to turn on");
			setting = false;
			DataVault.Set(currentMode.ToLower(), setting);
		} else {
			DataVault.Set("active_mode", "Tap to turn off");
			setting = true;
			DataVault.Set(currentMode.ToLower(), setting);
		}
	}
	
	// Update is called once per frame
	void OnDestroy () {
		GestureHelper.onTap -= tapHandler;
		GestureHelper.onSwipeDown -= downHandler;
	}
}
