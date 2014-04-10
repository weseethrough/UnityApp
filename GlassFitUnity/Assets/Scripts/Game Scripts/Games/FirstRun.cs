using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class FirstRun : GameBase {
	
	const float MIN_PACE = 1.5f;
	const float MAX_PACE = 6.0f;
	const int NUM_PACES = 6;
	
	private bool runReadyToStart = false;
	private PositionController runnerController;
	public TargetController runnerObj;

	private bool indoorRaceYourself = false;
	private bool indoorComplete = false;
	private AudioSource chime;
	private int numLaps = 0;

	// Use this for initialization
	public override void Start () {

		if(runnerObj != null)
		{
			runnerObj.GetComponent<FirstRaceOpponenet>().enabled = false;
			runnerObj.GetComponent<FirstRaceIndoorOpponent>().enabled = false;

		}
		
		base.Start ();
				
		UnityEngine.Debug.Log("FirstRun: Start");

		float fInterval = (MAX_PACE - MIN_PACE) / NUM_PACES;
		
		Platform.Instance.ResetTargets();
				
		chime = GetComponent<AudioSource>();
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
				FirstRaceIndoorOpponent indoorOpponent = runnerObj.GetComponent<FirstRaceIndoorOpponent>();
				indoorOpponent.setGame(this);
				runnerController = indoorOpponent;
				UnityEngine.Debug.Log("FirstRun: runner is indoor opponent");
				//runnerObj.GetComponent<FirstRaceOpponenet>().enabled = false;
			}
			else
			{
				runnerController = runnerObj.gameObject.GetComponent<FirstRaceOpponenet>();
				//runnerObj.GetComponent<FirstRaceIndoorOpponent>().enabled = false;
			}
			runnerController.enabled = true;
			
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
	}

	public void onLapComplete()
	{
		numLaps++;
		//show appropriate notification
		if(numLaps == 1)
		{
			FlowState.FollowFlowLinkNamed("RaceIndoorExit");
			StartCoroutine(GoBack());
		}
		else
		{
			FlowState.FollowFlowLinkNamed("IndoorCompleteExit");
			if(runnerObj.GetDistanceBehindTarget() < 0)
			{
				DataVault.Set("first_result", "You Won!");
			}
			else
			{
				DataVault.Set("first_result", "You Lost");
			}
			DataVault.Set("first_desc", "Now try again!");
			StartCoroutine(GoBack());
		}

		//chime
		if(chime != null)
		{
			chime.Play();
		}

		//reset runner position to player's current pos
		runnerObj.setRealWorldDist((float)Platform.Instance.LocalPlayerPosition.Distance);

	}
	
	protected override double GetDistBehindForHud ()
	{
		if(runnerController != null) {
			return runnerObj.GetDistanceBehindTarget();
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