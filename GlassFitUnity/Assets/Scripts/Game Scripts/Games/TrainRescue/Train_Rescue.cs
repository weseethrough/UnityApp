using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Train_Rescue : GameBase {
	
	public const string GAMESTATE_FLYTHROUGH = "flythrough";
	
	bool finished = false;
	
	protected TrainController_Rescue train;
	public GameObject trainObject = null;
	public GameObject damsel = null;
	public CameraPath openingFlythroughPath = null;
	
	float junctionSpacing = 250.0f;
	bool bFailed = false;
	
	//list of track pieces we create for the flythrough.
	protected List<GameObject> extraTrackPieces;
	
	bool bWaitedForSubtitleTimeOut = false;
	// Use this for initialization
	void Start () {
		base.Start();
		
		train = trainObject.GetComponent<TrainController_Rescue>();
		
		//clear strings
		DataVault.Set("train_subtitle", " ");
		DataVault.Set("game_message", " ");
		
		//set flag so that trophy is shown if we win
		DataVault.Set("showFinishTrophy", true);
		
		//set up a series of junctions for the train
		//beginning at 200m
		
		float junctionDist = 75.0f;
		
		selectedTrack = (Track)DataVault.Get("current_track");
		
//		try {
//			if(selectedTrack != null) {
//				finish = (int)selectedTrack.distance;
//			} else {
//				finish = (int)DataVault.Get("finish");
//			}
//		} catch(Exception e) {
//			finish = 5000;	
//		}
		
		UnityEngine.Debug.Log("Train: finish = " + finish);
		
		bool bDoneFirstOne = false;
		
//		while(junctionDist < 500.0f)
//		{
//			//create a junction
//			GameObject junctionObject = (GameObject)Instantiate(Resources.Load("TrainJunction"));
//			TrainTrackJunction junction = junctionObject.GetComponent<TrainTrackJunction>();
//			junction.setTrain(trainObject);
//			
////			if(!bDoneFirstOne)
////			{
////				junction.SwitchOnDetour();
////				bDoneFirstOne = true;
////			}
//			
//			junction.distancePosition = junctionDist;
//			
//			//move distance along
//			junctionDist += junctionSpacing;
//		}

		//create some additional tracks to put on the flythrough
		extraTrackPieces = new List<GameObject>();
		float totalTrackDistCovered = 500.0f;	//half of one track obj
		float trackPiecePosition = 0.0f;
		
		GameObject rightTrack = GameObject.Find("VirtualTrack");
		if(rightTrack == null)
		{
			UnityEngine.Debug.Log("Train: couldn't find right hand track");
		}
		
		GameObject leftTrack = GameObject.Find("VirtualTrainTrack");
		if(leftTrack == null)
		{
			UnityEngine.Debug.Log("Train: couldn't find left hand track");
		}
		
		while(totalTrackDistCovered <= finish + 500.0f)
		{
			//create another one, 1km further on
			trackPiecePosition += 1000.0f;
			
			//duplicate existing track
			GameObject newTrackPiece = GameObject.Instantiate(rightTrack) as GameObject;
			newTrackPiece.transform.localPosition = newTrackPiece.transform.localPosition + new Vector3(0,0,trackPiecePosition);
			extraTrackPieces.Add(newTrackPiece);
			
			GameObject newTrackPieceLeft = GameObject.Instantiate(leftTrack) as GameObject;
			newTrackPieceLeft.transform.localPosition = newTrackPieceLeft.transform.localPosition + new Vector3(0,0,trackPiecePosition);
			extraTrackPieces.Add(newTrackPieceLeft);
			
			totalTrackDistCovered += 1000.0f;
			
			//UnityEngine.Debug.Log("Train: Added another piece of track");
		}
		
	}
		
	protected override void OnEnterState (string state)
	{
		UnityEngine.Debug.Log("TrainRescue: entering state " + state);
		switch(state)
		{
		case GAMESTATE_COUNTING_DOWN:
			//start the specialised train rescue countdown
			StartCountdown();
			return;
			break;
			
		case GAMESTATE_FLYTHROUGH:
			//initiate the flythrough
			UnityEngine.Debug.Log("TrainRescue: Entering Flythrough state");
			if(openingFlythroughPath != null)
			{
				FlowState.FollowFlowLinkNamed("ToBlank");
				openingFlythroughPath.StartFollowingPath();
			}
			else
			{
				UnityEngine.Debug.LogWarning("TrainRescue: Couldn't find opening flythrough path");	
			}
			//start the music
			GameObject musicPlayer = GameObject.Find("MusicPlayer");
			AudioSource musicSource = (AudioSource)musicPlayer.GetComponent(typeof(AudioSource));
			musicSource.Play();
			return;
			break;
			
		case GAMESTATE_FINISHED:
			//don't go straight to end.
			StartCoroutine( ProgressToFinish() );
			return;
			break;
		}
		
		base.OnEnterState (state);
	}
	
	protected override void OnExitState (string state)
	{
		
		base.OnExitState (state);
	}
	
	protected override double GetDistBehindForHud ()
	{
		if(train == null)
		{
			train = trainObject.GetComponent<TrainController_Rescue>();
		}
		return train.GetDistanceBehindTarget();
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Platform.Instance.IsIndoor())
		{
			DataVault.Set("calories", "INDOOR");
		}
		
		switch(gameState)
		{
		case GAMESTATE_RUNNING:
			//check if the train has reached the end
			if(train.GetForwardDistance() > finish)
			{
				//finish the game
				bFailed = true;
				//set flag so that trophy isn't show
				DataVault.Set("showFinishTrophy", false);
				SetGameState(GAMESTATE_FINISHED);
			}
			break;
		case GAMESTATE_FLYTHROUGH:
			if(openingFlythroughPath.IsFinished())
			{
				SetGameState(GAMESTATE_COUNTING_DOWN);
			}
			break;
		}
		
		base.Update();
//		
	}
	
	protected override void OnFinishedGame ()
	{
		//Finished takes us to subtitle card.
		base.OnFinishedGame ();
	}

	public override void TriggerUserReady ()
	{
		if(gameState!=GAMESTATE_AWAITING_USER_READY)
		{
			UnityEngine.Debug.Log("GameBase: Received User Ready trigger, when not waiting for it");
			//move to countdown state
		}
		else
		{
			UnityEngine.Debug.Log("GameBase: Received User ready trigger");
			SetGameState(GAMESTATE_FLYTHROUGH);
		}
	}
	
	public void StartCountdown()
	{
		//delete extra track pieces
		foreach(GameObject piece in extraTrackPieces)
		{
			Destroy(piece);	
		}
		
//		base.SetReadyToStart(true);
		if(train == null)
		{
			train = trainObject.GetComponent<TrainController_Rescue>();
		}
		train.BeginRace();
		//progress flow to the normal HUD
		StartCoroutine(DoTrainRescueCountDown());
	}

	
	
//	//train game does its own version of the countdown
//	protected override bool shouldDoGameBaseCountdown ()
//	{
//		return false;
//	}
	
	IEnumerator DoTrainRescueCountDown()
	{
		UnityEngine.Debug.Log("Train:Starting Countdown Coroutine");
		
		//get to the HUD since all flow stems from here
		FlowState.FollowFlowLinkNamed("Begin");
		
		//small pause to allow flow to catch up
		yield return new WaitForSeconds(0.1f);
		
		for(int i=3; i>=0; i--)
		{
			//go to subtitle card
			UnityEngine.Debug.Log("Train: Following 'subtitle' connector");
			FlowState.FollowFlowLinkNamed("Subtitle");
			//set value for subtitle. 0 = GO
			string displayString = (i==0) ? "GO !" : i.ToString();
			DataVault.Set("train_subtitle", displayString);
			
			//wait half a second
			yield return new WaitForSeconds(0.5f);
			
			//return to cam
			UnityEngine.Debug.Log("Train: Following 'toblank' connector");
			FlowState.FollowFlowLinkNamed("ToBlank");
			
			//wait a second more, except after GO!
			if(i!=0)
			{
				yield return new WaitForSeconds(0.25f);
			}
			
		}
		
		yield return new WaitForSeconds(0.1f);
		
		UnityEngine.Debug.Log("Train: Following 'begin' connector");
		FlowState.FollowFlowLinkNamed("Begin");
		
		//play the train's bell sound effect
		train.soundBell();	
		
		//start the game
		SetGameState(GAMESTATE_RUNNING);;
	}

	
	IEnumerator ProgressToFinish()
	{
		UnityEngine.Debug.Log("TrainGame: Progressing to finish in 5 seconds");
		
		//go to subtitle card
		FlowState.FollowFlowLinkNamed("Subtitle");
		
		//set appropriate link subtitle
		if(bFailed || Platform.Instance.GetDistance() < finish)
		{
			DataVault.Set("train_subtitle", "\"Aaaaargh!\"");
		}
		else
		{
			DataVault.Set("train_subtitle", "\"My Hero!\"");
		}
		
		//wait for 5s then continue
		yield return new WaitForSeconds(5.0f);
		
		//now we are finished
		FinishGame();
		
	}
}
