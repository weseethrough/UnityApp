﻿using UnityEngine;
using System.Collections;

public class PurchaseListener : MonoBehaviour {
	
	private GestureHelper.OnTap tapHandler = null;
	
	private GestureHelper.OnBack backHandler = null;
	
	// Use this for initialization
	void Start () {
		tapHandler = new GestureHelper.OnTap(() => {
			PurchaseGame();
		});
		
		GestureHelper.onTap +=  tapHandler;
		
		backHandler = new GestureHelper.OnBack(() => {
			GoBack();
		});
		
		GestureHelper.onBack += backHandler;
	}
	
	void GoBack() 
	{
		//FlowState.FollowBackLink();
	}
	
	void PurchaseGame() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "MenuButton");
		
		if(gConnect != null) {
			Game current = (Game)DataVault.Get("actual_game");
			UnityEngine.Debug.Log("Purchase: Game bought");
			current.Unlock();
			fs.parentMachine.FollowConnection(gConnect);
		} else {
			UnityEngine.Debug.Log("Purchase: No connection found, cannot purchase");
		}
	}
	
	void Update() {
		if(Input.GetKeyDown(KeyCode.Escape)) {
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConnect = fs.Outputs.Find(r => r.Name == "MenuButton");
			
			if(gConnect != null) {
				fs.parentMachine.FollowConnection(gConnect);
			} else {
				UnityEngine.Debug.Log("Purchase: No connection found, cannot purchase");
			}
		}
	}
	
	// Update is called once per frame
	void OnDestroy() {
		GestureHelper.onTap -= tapHandler;
		GestureHelper.onBack -= backHandler;
	}
}
