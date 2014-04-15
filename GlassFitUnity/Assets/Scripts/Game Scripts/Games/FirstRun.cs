using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class FirstRun : GameBase {
	
	const float MIN_PACE = 1.5f;
	const float MAX_PACE = 6.0f;
	const int NUM_PACES = 6;
	
	private bool runReadyToStart = false;
	private TargetController runner;		//a runner object. This will be cloned around for various benchmark paces.
	public GameObject runnerObj;
	private float runnerHeadStartDist = 20.0f;
	
	public new Camera camera;
	//const float paceLabelYOffsetScreen = 0.0f;
	//const float paceLabelYOffsetWorld = 300.0f;
	
	bool shouldShowPaceLabels = false;
	const float showLabelMinRange = 0.1f;
	const float showLabelMaxRange = 500.0f;
	
	private bool indoorRaceYourself = false;
	
	private bool indoorComplete = false;
	
	private AudioSource chime;
	
	//public UINavProgressBar progressBar;
	
	bool hasResetGyros = false;  // possibly not needed? remove?
	
	// Use this for initialization
	public override void Start () {

		if(runnerObj != null)
		{
			runnerObj.GetComponent<FirstRaceOpponenet>().enabled = false;
			runnerObj.GetComponent<FirstRaceIndoorOpponent>().enabled = false;
			
		}
		
		base.Start ();
				
		UnityEngine.Debug.Log("FirstRun: Start");

		Platform.Instance.ResetTargets();
				
		chime = GetComponent<AudioSource>();

	}
	
	protected void SetRunnerVisible(bool visible)
	{
		runnerObj.SetActive(visible);	
	}

	
	protected override void OnEnterState(string state)
	{
		base.OnEnterState(state);	
	}
	
	protected override void OnExitState(string state)
	{
		switch(state)
		{
		case GAMESTATE_AWAITING_USER_READY:
			//substitute the opponent according to whether we are indoors or outdoors
			runReadyToStart = true;
			
		if(Platform.Instance.LocalPlayerPosition.IsIndoor())
			{
				runner = runnerObj.GetComponent<FirstRaceIndoorOpponent>();
				UnityEngine.Debug.Log("FirstRun: runner is indoor opponent");
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
			break;
		}
		base.OnExitState(state);
	}
	
	
	IEnumerator GoBack()
	{
		yield return new WaitForSeconds(2.0f);
		FlowState.FollowBackLink();
	}
	
	// Update is called once per frame
	public override void Update () {
		
		base.Update();
		
		if(runReadyToStart)
		{
			if(runner is FirstRaceIndoorOpponent) {
				double distance = Platform.Instance.LocalPlayerPosition.Distance;
				
				if(distance > 50 && distance < 100)
				{
					if(!indoorRaceYourself)
					{
						indoorRaceYourself = true;
						FlowState fs = FlowStateMachine.GetCurrentFlowState();
						GConnector gConnect = fs.Outputs.Find(r => r.Name == "RaceIndoorExit");
						if(gConnect != null)
						{
							if(chime != null)
							{
								chime.Play();
							}
							fs.parentMachine.FollowConnection(gConnect);
							StartCoroutine(GoBack());
						}
						else
						{
							UnityEngine.Debug.Log("FirstRun: couldn't find " + gConnect.Name);
						}
						runner.SetHeadstart((float)Platform.Instance.LocalPlayerPosition.Distance);
						//(runner as FirstRaceIndoorOpponent).SetRunnerSpeed();
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
							}
							else
							{
								DataVault.Set("first_result", "You Lost");
							}
							DataVault.Set("first_desc", "Now try again!");
							if(chime != null)
							{
								chime.Play();
							}
							fs.parentMachine.FollowConnection(gConnect);
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

	public override GConnector GetFinalConnection ()
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		return fs.Outputs.Find(r => r.Name == "FinishButton");
		
	}

}