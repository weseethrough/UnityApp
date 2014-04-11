using UnityEngine;
using System.Collections;
using System;

public class ModeControl : MonoBehaviour {

	private UIAtlas atlas;
	private UISprite sprite;

	private bool setting = false;
	
	GestureHelper.OnTap tapHandler = null;
	
	GestureHelper.OnBack backHandler = null;
	
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

		if(setting) {
			DataVault.Set("active_mode", "Tap to turn off");
		} else {
			DataVault.Set("active_mode", "Tap to turn on");
		}
			
		tapHandler = new GestureHelper.OnTap(() => {
			SetMode();
		});
		GestureHelper.onTap += tapHandler;
			
		backHandler = new GestureHelper.OnBack(() => {
			BackToMenu();
		});
		
		GestureHelper.onBack += backHandler;
			
	}
	
	void BackToMenu() {
		FlowState.FollowFlowLinkNamed("MenuButton");
	}
	
	void SetMode() {
		UnityEngine.Debug.Log("Mode: screen tapped");

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
		GestureHelper.onBack -= backHandler;
	}
}
