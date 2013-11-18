using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class SettingsPanel : Panel {
	
	public SettingsPanel(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) {
	}
	
	public SettingsPanel() {}
	
	protected override void Initialize()
    {
        base.Initialize();
//		if(Platform.Instance != null) {
//			Platform.Instance.stopTrack();
//		}
	}
	
	public override void OnClick(FlowButton button)
	{
		base.OnClick(button);
		
		SettingsScreen ss = (SettingsScreen) GameObject.FindObjectOfType(typeof(SettingsScreen));
		if(ss != null) 
		{
			switch(button.name)
			{
			case "RunnerButton":
				ss.SetTarget(SettingsScreen.Targets.Runner);
				break;
				
			case "ZombieButton":
				ss.SetTarget(SettingsScreen.Targets.Zombie);
				break;
				
			case "EagleButton":
				ss.SetTarget(SettingsScreen.Targets.Eagle);
				break;
				
			case "TrainButton":
				ss.SetTarget(SettingsScreen.Targets.Train);
				break;
				
			case "IndoorButton":
				ss.SetIndoor();
				
				break;
				
			case "ServerButton":
//				Debug.Log("SettingsPanel: ServerButton clicked");
	            GConnector gConect = Outputs.Find(r => r.Name == button.name);
				// Follow connection once authentication has returned asynchronously
				Platform.OnAuthenticated handler = null;
				handler = new Platform.OnAuthenticated((authenticated) => {
//					Debug.Log("SettingsPanel: ServerButton authenticated");
					if (authenticated) {
						Platform.Instance.syncToServer();
						parentMachine.FollowConnection(gConect);
					}
					Platform.Instance.onAuthenticated -= handler;
				});
				Platform.Instance.onAuthenticated += handler;	
				// Trigger authentication
				Platform.Instance.authorize("any", "login");				
//				Debug.Log("SettingsPanel: ServerButton run");
				break;
				
			case "GetTrackButton":
				Platform.Instance.getTracks();
				GameObject.Find("TrackSelect").renderer.enabled = true;
				break;
				
			case "BackMainButton":
				ss.Back();
				GameObject h = GameObject.Find("blackPlane");
				h.renderer.enabled = false;
				
				//h = GameObject.Find("minimap");
				//h.renderer.enabled = true;
				
				h = GameObject.Find("minimap2");
				h.renderer.enabled = true;
				break;	
				
			case "FriendButton": 
				Debug.Log("FriendButton clicked");
				break;
//			case "NextButton":
//				
//				break;
//				
//			case "PrevButton":
//				
//				break;
//				
//			case "SetTrackButton":
//				
//				break;

//			case "BackSettingsButton":
//				
//				break;		
			
			}
		}
	}
}
