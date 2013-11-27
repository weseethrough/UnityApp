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
		// Get the map texture
		GameObject go = GameObject.Find("MapTexture");
		Debug.Log ("TrackPanel: GameObject");
		// Get the track select object
		trackHandler = (TrackSelect)go.GetComponent(typeof(TrackSelect));
		Debug.Log ("TrackPanel: trackHandler");
	}
	
	public override void OnClick(FlowButton button)
	{
		base.OnClick(button);
		
		Debug.Log ("TrackPanel: Button " + button.name);
		switch(button.name)
		{
			// Increase the current track
			case "NextButton":
				Platform.Instance.currentTrack++;
				break;
				
			// Decrease the current track
			case "PrevButton":
				Platform.Instance.currentTrack--;
				break;
				
			// Set the current track
			case "SetTrackButton":
				// NOTE: This button does nothing. P.I.currentTrack is used directly in TrackSelect.
				break;
			
			case "BackSettingsButton":
				//GameObject.Find("TrackSelect").renderer.enabled = false;
				break;		
			
			// Set the share button
			case "ShareButton":
				if (trackHandler == null) Startup ();
				if (!Platform.Instance.HasPermissions("facebook", "share")) {
					Platform.Instance.Authorize("facebook", "share");
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