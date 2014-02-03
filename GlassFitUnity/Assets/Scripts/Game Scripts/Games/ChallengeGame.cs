using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChallengeGame : GameBase {
	
	public GameObject runnerHolder;
	
	// Use this for initialization
	void Start () {
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
		foreach (TargetTracker tracker in trackers) {
			TargetController controller = runnerHolder.GetComponent<TargetController>();
			controller.SetTracker(tracker);
			controller.SetLane(lane++);
		}
		
		SetVirtualTrackVisible(true);
		//SetReadyToStart(true);
	}
	
	public override GConnector GetFinalConnection()
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		ChallengeNotification current = (ChallengeNotification)DataVault.Get("current_challenge_notification");
		if(Platform.Instance.Time() < current.GetTime()) { 
			DataVault.Set("challenge_result", "You beat " + (string)DataVault.Get("challenger"));
		} else {
			DataVault.Set("challenge_result", (string)DataVault.Get("challenger") + " beat you!");
		}
		current.SetRead();
		return fs.Outputs.Find(r => r.Name == "ChallengeExit");
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
	}
}
