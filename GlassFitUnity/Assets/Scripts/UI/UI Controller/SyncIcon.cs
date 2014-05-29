using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SyncIcon : MonoBehaviour {
	
    // TODO strip out icon rotation and reference TextureRotation instead.

	private float rotation = 0f;
	
    private NetworkMessageListener.OnAuthenticated authHandler = null;
    private NetworkMessageListener.OnSync syncHandler = null;
    private NetworkMessageListener.OnSyncProgress syncProgressHandler = null;
	
	// Use this for initialization
	void Start () {
		UnityEngine.Debug.Log("SyncIcon: in start function");
		if(Platform.Instance.IsPluggedIn() && Platform.Instance.HasInternet()) {
			UnityEngine.Debug.Log("SyncIcon: setting auth handler");
            authHandler = new NetworkMessageListener.OnAuthenticated((errors) => {
                bool authenticated = errors.Count == 0;

                Platform.Instance.NetworkMessageListener.onAuthenticated -= authHandler;

				UnityEngine.Debug.Log("SyncIcon: checking if authenticated");
				if (!authenticated) {
                    var errorDetail = new System.Text.StringBuilder();
                    foreach (KeyValuePair<string, IList<string>> entry in errors)
                    {
                        foreach (string v in entry.Value)
                        {
                            errorDetail.Append(entry.Key);
                            errorDetail.Append(" ");
                            errorDetail.AppendLine(v);
                        }
                    }
                    UnityEngine.Debug.LogError("SyncIcon: auth failure: " + errorDetail);

					MessageWidget.AddMessage("ERROR", "Could not authenticate", "settings");
					GoToGame();
					return;
				}

				UnityEngine.Debug.Log("SyncIcon: setting sync progress handler");
                syncProgressHandler = new NetworkMessageListener.OnSyncProgress((message) => {
					MessageWidget.AddMessage("Syncing", message, "settings");
				});
				UnityEngine.Debug.Log("SyncIcon: setting sync handler");
                syncHandler = new NetworkMessageListener.OnSync((message) => {
                    Platform.Instance.NetworkMessageListener.onSyncProgress -= syncProgressHandler;
                    Platform.Instance.NetworkMessageListener.onSync -= syncHandler;
					GoToGame();
				});
                Platform.Instance.NetworkMessageListener.onSync += syncHandler;

				UnityEngine.Debug.Log("SyncIcon: calling sync to server");
				Platform.Instance.SyncToServer();
			});
            Platform.Instance.NetworkMessageListener.onAuthenticated += authHandler;
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

        Platform.Instance.NetworkMessageListener.onSync -= syncHandler;
	}
	
	// Update is called once per frame
	void Update () {
		rotation -= 360.0f * Time.deltaTime;
		
		transform.rotation = Quaternion.Euler(0, 0, rotation);
	}
}
