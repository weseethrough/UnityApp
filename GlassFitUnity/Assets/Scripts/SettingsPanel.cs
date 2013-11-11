using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class SettingsPanel : Panel {
	
	public SettingsPanel(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) {
	}
	
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
				ss.GetServer();
				break;
				
			case "GetTrackButton":
				Platform.Instance.getTracks();
				break;
				
			case "BackMainButton":
				ss.Back();
				GameObject h = GameObject.Find("blackPlane");
				h.renderer.enabled = false;
				
				h = GameObject.Find("minimap");
				h.renderer.enabled = true;
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
