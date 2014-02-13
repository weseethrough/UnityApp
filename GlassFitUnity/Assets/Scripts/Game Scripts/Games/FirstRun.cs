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
	private TargetController runner;		//a runner object. This will be cloned around for various benchmark paces.
	public GameObject runnerObj;
	private float runnerHeadStartDist = 20.0f;
	
	public Camera camera;
	//const float paceLabelYOffsetScreen = 0.0f;
	//const float paceLabelYOffsetWorld = 300.0f;
	
	bool shouldShowPaceLabels = false;
	const float showLabelMinRange = 0.1f;
	const float showLabelMaxRange = 500.0f;
	
	private bool indoorRaceYourself = false;
	
	private bool indoorComplete = false;
	
	//public UINavProgressBar progressBar;
	
	bool hasResetGyros = false;  // possibly not needed? remove?
	
	// Use this for initialization
	void Start () {

		base.Start ();
				
		UnityEngine.Debug.Log("FirstRun: Start");
		
		if(runnerObj != null)
		{
			runnerObj.GetComponent<FirstRaceOpponenet>().enabled = false;
			runnerObj.GetComponent<FirstRaceIndoorOpponent>().enabled = false;
			
		}
		
		//hide virtual track to begin with
		SetVirtualTrackVisible(false);
		SetRunnerVisible(true);
		
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
		
		
		
		//create actors for each target tracker
		//InstantiateActors();
		
		//SetReadyToStart(true);
	}
	
	protected void SetRunnerVisible(bool visible)
	{
		runnerObj.SetActive(visible);	
	}

	
	public override void SetReadyToStart (bool ready)
	{
		base.SetReadyToStart(ready);
		runReadyToStart = ready;
		
		if(Platform.Instance.IsIndoor())
		{
			runner = runnerObj.GetComponent<FirstRaceIndoorOpponent>();
			//runnerObj.GetComponent<FirstRaceOpponenet>().enabled = false;
		}
		else
		{
			runner = runnerObj.GetComponent<FirstRaceOpponenet>();
			//runnerObj.GetComponent<FirstRaceIndoorOpponent>().enabled = false;
		}
		runner.enabled = true;
		
		if(runner is FirstRaceOpponenet) {
			runner.SetHeadstart(20.0f);
		}else if(runner is FirstRaceIndoorOpponent){
			runner.SetHeadstart(50.0f);
		}
		
		SetVirtualTrackVisible(true);
	}
	
	IEnumerator GoBack()
	{
		yield return new WaitForSeconds(2.0f);
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		fs.parentMachine.FollowBack();
	}
	
	// Update is called once per frame
	void Update () {
		
		if(runReadyToStart)
		{
			base.Update();
			
			if(runner is FirstRaceIndoorOpponent) {
				double distance = Platform.Instance.Distance();
				
				if(distance > 50 && distance < 100)
				{
					if(!indoorRaceYourself)
					{
						indoorRaceYourself = true;
						FlowState fs = FlowStateMachine.GetCurrentFlowState();
						GConnector gConnect = fs.Outputs.Find(r => r.Name == "RaceIndoorExit");
						if(gConnect != null)
						{
							fs.parentMachine.FollowConnection(gConnect);
							StartCoroutine(GoBack());
						}
						else
						{
							UnityEngine.Debug.Log("FirstRun: couldn't find " + gConnect.Name);
						}
						runner.SetHeadstart((float)Platform.Instance.Distance());
						(runner as FirstRaceIndoorOpponent).SetRunnerSpeed();
					}
				}
				else if(distance > 100)
				{
					if(!indoorComplete)
					{
						indoorComplete = true;
						
						FlowState fs = FlowStateMachine.GetCurrentFlowState();
						GConnector gConnect = fs.Outputs.Find(r => r.Name == "IndoorCompleteExit");
						if(gConnect != null)
						{
							if(runner.GetDistanceBehindTarget() < 0)
							{
								DataVault.Set("first_result", "You Won!");
								DataVault.Set("first_desc", "You beat your previous time!");
							}
							else
							{
								DataVault.Set("first_result", "You Lost");
								DataVault.Set("first_desc", "You previous time was faster");
							}
							fs.parentMachine.FollowConnection(gConnect);
							runner.enabled = false;
							StartCoroutine(GoBack());
						}
						else
						{
							UnityEngine.Debug.Log("FirstRun: couldn't find " + gConnect.Name);
						}
					}
				}
			}
		}
	}
	
	protected override double GetDistBehindForHud ()
	{
		if(runner != null) {
			return runner.GetDistanceBehindTarget();
		}
		else
		{
			return 0;
		}
	}

//	public override GConnector GetFinalConnection ()
//	{
//		FlowState fs = FlowStateMachine.GetCurrentFlowState();
//		if((string)DataVault.Get("race_type") == "race") {
//			return fs.Outputs.Find(r => r.Name == "FinishButton");
//		} else {
//			return fs.Outputs.Find(r => r.Name == "TutorialExit");
//		}
//	}
		
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
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "FinishButton"); 
//		if((string)DataVault.Get("race_type") == "race") {
//			gConnect = fs.Outputs.Find(r => r.Name == "MenuExit");
//		} else {
//			gConnect = fs.Outputs.Find(r => r.Name == "TutorialExit");
//		}
		
		if(gConnect != null) {
			GestureHelper.onSwipeDown -= downHandler;
			GestureHelper.onTap -= tapHandler;
			fs.parentMachine.FollowConnection(gConnect);
			AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
		} else {
			UnityEngine.Debug.Log("FirstRun: Error finding quit exit");
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