using UnityEngine;
using System.Collections;

public class ModeControl : MonoBehaviour {

	private UIAtlas atlas;
	private UISprite sprite;
	private bool rearview = false;
	
	GestureHelper.OnTap tapHandler = null;
	
	GestureHelper.OnSwipeLeft leftHandler = null;
	
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
		
		rearview = (bool)DataVault.Get("rearview");
		UnityEngine.Debug.Log("Mode: rearview is set to: " + rearview.ToString());
		if(rearview) {
			DataVault.Set("active_mode", "Press to turn off");
			rearview = true;
		} else {
			DataVault.Set("active_mode", "Press to turn on");
			rearview = false;
		}
		
		tapHandler = new GestureHelper.OnTap(() => {
			SetRearview();
		});
		GestureHelper.onTap += tapHandler;
		
		leftHandler = new GestureHelper.OnSwipeLeft(() => {
			BackToMenu();
		});
		GestureHelper.swipeLeft += leftHandler;
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
	
	void SetRearview() {
		UnityEngine.Debug.Log("Mode: screen tapped");
		if(rearview) {
			DataVault.Set("active_mode", "Press to turn on");
			rearview = false;
			DataVault.Set("rearview", rearview);
		} else {
			DataVault.Set("active_mode", "Press to turn off");
			rearview = true;
			DataVault.Set("rearview", rearview);
		}
	}
	
	// Update is called once per frame
	void OnDestroy () {
		GestureHelper.onTap -= tapHandler;
		GestureHelper.swipeLeft -= leftHandler;
	}
}
