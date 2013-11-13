using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class TrackPanel : Panel {
	
	public TrackPanel(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) {
	}
	
	public TrackPanel() {}
	
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
		
		switch(button.name)
		{
			case "NextButton":
				Platform.Instance.getNextTrack();
				break;
				
			case "PrevButton":
				Platform.Instance.getPreviousTrack();
				break;
				
			case "SetTrackButton":
				Platform.Instance.setTrack();
				break;

			case "BackSettingsButton":
				GameObject.Find("TrackSelect").renderer.enabled = false;
				break;		
		}
	}
}