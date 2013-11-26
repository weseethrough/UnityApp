using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class TrackPanel : Panel {
	private TrackSelect trackHandler = null;
	
	public TrackPanel(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) {
	}
	
	public TrackPanel() {}
	
	protected override void Initialize()
    {
        base.Initialize();
	}

	protected void Startup() {
		Debug.Log ("TrackPanel: Startup");
		GameObject go = GameObject.Find("MapTexture");
		Debug.Log ("TrackPanel: GameObject");
		trackHandler = (TrackSelect)go.GetComponent(typeof(TrackSelect));
		Debug.Log ("TrackPanel: trackHandler");
	}
	
	public override void OnClick(FlowButton button)
	{
		base.OnClick(button);
		
		Debug.Log ("TrackPanel: Button " + button.name);
		switch(button.name)
		{
			case "NextButton":
				Platform.Instance.currentTrack++;
				break;
				
			case "PrevButton":
				Platform.Instance.currentTrack--;
				break;
				
			case "SetTrackButton":
				Platform.Instance.setTrack();
				break;

			case "BackSettingsButton":
				//GameObject.Find("TrackSelect").renderer.enabled = false;
				break;		
			
			case "ShareButton":
				if (trackHandler == null) Startup ();
				if (!Platform.Instance.hasPermissions("facebook", "share")) {
					Platform.Instance.authorize("facebook", "share");
					// TODO: Async continue
					break;
				}
				Track track = trackHandler.CurrentTrack();
				Platform.Instance.QueueAction(string.Format(@"{{
					'action' : 'share',
					'provider' : 'facebook',
					'message' : 'Dummy message',
					'track' : [{0}, {1}]
				}}", track.deviceID, track.trackID).Replace("'", "\""));		
				Debug.Log ("Track: [" + track.deviceID + "," + track.trackID + "] shared to Facebook");
			
				break;
		}
		Debug.Log("TrackPanel: track " + Platform.Instance.currentTrack);
	}
}