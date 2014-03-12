using UnityEngine;
using System.Collections;
using System;

public class SyncIcon : MonoBehaviour {
	
	private float rotation = 0f;
	
	private Platform.OnAuthenticated authHandler = null;
	private Platform.OnSync syncHandler = null;
	private Platform.OnSyncProgress syncProgressHandler = null;
	
	// Use this for initialization
	void Start () {
		
		if(Platform.Instance.IsPluggedIn() && Platform.Instance.HasWifi()) {
			authHandler = new Platform.OnAuthenticated((authenticated) => {
				Platform.Instance.onAuthenticated -= authHandler;
				
				if (!authenticated) {
					MessageWidget.AddMessage("ERROR", "Could not authenticate", "settings");
					GoToGame();
					return;
				}
				
				syncProgressHandler = new Platform.OnSyncProgress((message) => {
					MessageWidget.AddMessage("Syncing", message, "settings");
				});
				syncHandler = new Platform.OnSync((message) => {
					Platform.Instance.onSyncProgress -= syncProgressHandler;
					Platform.Instance.onSync -= syncHandler;
					GoToGame();
				});
				Platform.Instance.onSync += syncHandler;
				
				Platform.Instance.SyncToServer();
			});
			Platform.Instance.onAuthenticated += authHandler;
			Platform.Instance.Authorize("any", "login");
		} else {
			GoToGame();
		}
	}
	
//	IEnumerator SyncServer() {
//		if(Platform.Instance.IsPluggedIn()) {
//			
//			Platform.Instance.SyncToServer();
//			
//			yield return new WaitForEndOfFrame();
//			
//			GoToGame();
//			
//		} else {
//			GoToGame();
//		}
//	}
	
	void GoToGame() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect;
		
		//UnityEngine.Debug.Log("SyncIcon - Datavault value is " + DataVault.Get("test_string").ToString());
		
		bool tutorial = Convert.ToBoolean(DataVault.Get("tutorial_complete"));
		
		if(tutorial) {
			FlowState.FollowFlowLinkNamed("GameExit");
		}
		else
		{
			FlowState.FollowFlowLinkNamed("TutorialExit");
		}

		Platform.Instance.onSync -= syncHandler;
	}
	
	// Update is called once per frame
	void Update () {
		rotation -= 360.0f * Time.deltaTime;
		
		transform.rotation = Quaternion.Euler(0, 0, rotation);
	}
}
