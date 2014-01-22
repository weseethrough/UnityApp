using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;




public class FirstRun : GameBase {
	
	const float MIN_PACE = 1.5f;
	const float MAX_PACE = 6.0f;
	const int NUM_PACES = 6;
	
	protected GestureHelper.OnSwipeRight swipeHandler = null;
	private GestureHelper.OnSwipeLeft leftHandler = null;

	
	private bool runReadyToStart = false;
	public FirstRaceOpponenet runner;		//a runner object. This will be cloned around for various benchmark paces.
	
	private float runnerHeadStartDist = 20.0f;
	
	public Camera camera;
	//const float paceLabelYOffsetScreen = 0.0f;
	//const float paceLabelYOffsetWorld = 300.0f;
	
	bool shouldShowPaceLabels = false;
	const float showLabelMinRange = 0.1f;
	const float showLabelMaxRange = 500.0f;
	
	//public UINavProgressBar progressBar;
	
	bool hasResetGyros = false;
	
	// Use this for initialization
	void Start () {

		base.Start ();
				
		UnityEngine.Debug.Log("FirstRun: Start");
		
		//hide virtual track to begin with
		SetVirtualTrackVisible(false);
		SetRunnerVisible(false);
		
		//create target trackers for a few different paces
		float fInterval = (MAX_PACE - MIN_PACE) / NUM_PACES;
		
		Platform.Instance.ResetTargets();
				
//		for(float TotalTimePace = 2.0f; TotalTimePace <= 10.0f; TotalTimePace += 1.0f)
//		{
//			float TotalSeconds = TotalTimePace * 60;	
//			
//			float speed = finish/TotalSeconds;
//			
//			TargetTracker tracker = Platform.Instance.CreateTargetTracker(speed);
//		}
		
		GameObject actor;
		
		runner.setHeadStart(20.0f);
		
		//create actors for each target tracker
		//InstantiateActors();
		
		
	}
	
	protected void SetRunnerVisible(bool visible)
	{
		runner.gameObject.SetActive(visible);	
	}

	
	public override void SetReadyToStart (bool ready)
	{
		base.SetReadyToStart(ready);
		runReadyToStart = ready;
		SetRunnerVisible(true);
	}
	
	// Update is called once per frame
	void Update () {
		
		if(runReadyToStart)
		{
			base.Update();
		}
	}
	
	protected override double GetDistBehindForHud ()
	{
		return runner.GetDistanceBehindTarget();
	}

	public override GConnector GetFinalConnection ()
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		return fs.Outputs.Find(r => r.Name == "TutorialExit");
	}
		
	void OnGUI() {
		if(runReadyToStart) {
			base.OnGUI();
		}
		
		//if the user has swiped down to quit, and is seeing the quit confirmation box, dont' show anything here.
		if(maybeQuit) {
			base.OnGUI();
		}
	}
//	
//	public override void GameHandleTap ()
//	{
//		if(started)
//		{
//			base.GameHandleTap ();
//		}
//	}
	
	protected override void OnUnpause ()
	{

	}
	
	public override void QuitGame ()
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "TutorialExit");
		if(gConnect != null) {
			GestureHelper.onSwipeDown -= downHandler;
			GestureHelper.onTap -= tapHandler;
			fs.parentMachine.FollowConnection(gConnect);
			AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
		} else {
			UnityEngine.Debug.Log("FirstRun: Error finding tutorial exit");
		}
	}
	
	void OnDestroy() {
		//deregister handlers
		if(swipeHandler != null)
		{
			GestureHelper.onSwipeRight -= swipeHandler;
		}
		if(leftHandler != null)
		{
			GestureHelper.onSwipeLeft -= leftHandler;
		}
	}
	
//	private void InstantiateActors() {
//		//create an actor for each active target tracker
//		List<TargetTracker> trackers = Platform.Instance.targetTrackers;
//		int lane = 1;
//		foreach (TargetTracker tracker in trackers) {
//			GameObject actor = Instantiate(runner) as GameObject;
//			TargetController controller = actor.GetComponent<TargetController>();
//			controller.SetLane(1);
//			controller.SetTracker(tracker);
//			controller.SetLane(lane++);
//			//actor.SetActive(true);
//			actors.Add(actor);
//			
//			//determine pace and set string
//			float speed = tracker.PollCurrentSpeed();
//			long totalTime = (long)((float)finish/speed)*1000;
//			//UnityEngine.Debug.Log("FirstRun: Speed = " + speed);
//			//UnityEngine.Debug.Log("FirstRun: totalTime = " + totalTime);
//			string paceString = TimestampMMSSFromMS(totalTime) + "min/km";
//			//UnityEngine.Debug.Log("FirstRun: pace = " + paceString);
//			controller.overheadLabelString = paceString;
//		}
//		
//	}
	
		
		
}