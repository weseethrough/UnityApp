using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class MainPanel : Panel {

	public MainPanel() {}
	public MainPanel(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) {
	}
	
	protected override void Initialize()
    {
        base.Initialize();
	}
	
	public override void OnClick (FlowButton button)
	{
		UnityEngine.Debug.Log("Panel: Button is pressed");
		base.OnClick (button);
		
		switch(button.name) {
		case "SettingsButton":
			UnityEngine.Debug.Log("Panel: finding game object");
			
			GameObject h = GameObject.Find("blackPlane");
			UnityEngine.Debug.Log("Panel: Object found");
			h.renderer.enabled = true;
			
			h = GameObject.Find("minimap");
			h.renderer.enabled = false;
			
			UnityEngine.Debug.Log("Panel: Renderer Enabled");
			break;
		}
	}
	
	public override void OnPress(FlowButton button, bool isDown) 
	{
		base.OnPress(button, isDown);
		
		
	}
}
