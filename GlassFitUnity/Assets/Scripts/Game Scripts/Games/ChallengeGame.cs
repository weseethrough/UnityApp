using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChallengeGame : GameBase {
	
	public RYWorldObject runnerHolder;
	
	// Use this for initialization
	public override void Start () {
		base.Start();
		
		if(finish == 0) 
		{
			finish = 500;
		}
		
		Platform.Instance.ResetTargets();
		
		Platform.Instance.CreateTargetTracker(selectedTrack.deviceId, selectedTrack.trackId);
			
		DataVault.Set("ending_bonus", " ");
		
		List<TargetTracker> trackers = Platform.Instance.targetTrackers;
		int lane = 1;
		Vector3 newRunnerPos;

		foreach (TargetTracker tracker in trackers) {
			TargetTrackerPositionController controller = runnerHolder.GetComponent<TargetTrackerPositionController>();
			controller.tracker = tracker;
			//runnerHolder.SetLane(lane++);
		}
		
		SetVirtualTrackVisible(true);
		//SetReadyToStart(true);
	}
	
	public override GConnector GetFinalConnection()
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		ChallengeNotification current = (ChallengeNotification)DataVault.Get("current_challenge_notification");
		if(Platform.Instance.LocalPlayerPosition.Time < current.GetTime()) { 
			DataVault.Set("challenge_result", "You beat " + (string)DataVault.Get("challenger"));
		} else {
			DataVault.Set("challenge_result", (string)DataVault.Get("challenger") + " beat you!");
		}
		current.SetRead();
		return fs.Outputs.Find(r => r.Name == "ChallengeExit");
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();
	}
}
