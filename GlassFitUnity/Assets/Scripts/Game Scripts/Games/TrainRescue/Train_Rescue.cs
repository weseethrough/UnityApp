using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Train_Rescue : GameBase {
	
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
		
		//Platform.Instance.LocalPlayerPosition.SetIndoor(true);
		
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
		base.Update();
		
		if(Platform.Instance.LocalPlayerPosition.IsIndoor())
		{
			DataVault.Set("calories", "INDOOR");
		}
		
		if(!finished && !hasEnded)
		{
			//check if the train has reached the end
			if(train.GetForwardDistance() > finish && !finished)
			{
				//finish the game
				bFailed = true;
				//set flag so that trophy isn't show
				DataVault.Set("showFinishTrophy", false);
				FinishGame();
				finished = true;
			}
		}
		
		//check if the flythrough is complete
		if(!readyToStart)
		{
			if(openingFlythroughPath.IsFinished())
			{
				StartCountdown();
			}
		}
		
	}
	
	public override void SetReadyToStart (bool ready)
	{		
		if(openingFlythroughPath != null)
		{
			openingFlythroughPath.StartFollowingPath();	
		}
		else
		{
			UnityEngine.Debug.LogError("Train: Don't have camera path set!");	
		}
		
		
		//start the music
		GameObject musicPlayer = GameObject.Find("MusicPlayer");
		AudioSource musicSource = (AudioSource)musicPlayer.GetComponent(typeof(AudioSource));
		musicSource.Play();
	}
	
	public void StartCountdown()
	{
		//delete extra track pieces
		foreach(GameObject piece in extraTrackPieces)
		{
			Destroy(piece);	
		}
		
		base.SetReadyToStart(true);
		if(train == null)
		{
			train = trainObject.GetComponent<TrainController_Rescue>();
		}
		train.BeginRace();
		//progress flow to the normal HUD
		//FollowConnectorNamed("Begin");
		StartCoroutine(DoCountDown());
	}
	
	//train game does its own version of the countdown
	protected override bool shouldDoGameBaseCountdown ()
	{
		return false;
	}
	
	IEnumerator DoCountDown()
	{
		UnityEngine.Debug.Log("Train:Starting Countdown Coroutine");
		for(int i=3; i>=0; i--)
		{
			//go to subtitle card
			UnityEngine.Debug.Log("Train: Following 'subtitle' connector");
			FollowConnectorNamed("Subtitle");
			//set value for subtitle. 0 = GO
			string displayString = (i==0) ? "GO !" : i.ToString();
			DataVault.Set("train_subtitle", displayString);
			
			//wait half a second
			yield return new WaitForSeconds(1.0f);
			
			//return to cam
			UnityEngine.Debug.Log("Train: Following 'toblank' connector");
			FollowConnectorNamed("ToBlank");
			
			//wait a second more, except after GO!
			if(i!=0)
			{
				yield return new WaitForSeconds(1.5f);
			}
			
		}
		
		yield return new WaitForSeconds(0.1f);
		
		UnityEngine.Debug.Log("Train: Following 'begin' connector");
		FollowConnectorNamed("Begin");
		
		//play the train's bell sound effect
		train.soundBell();	
		
		//start the game
		StartRace();
	}
	
	public void FollowConnectorNamed(string name)
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find( r => r.Name == name );
		if(gConnect != null)
		{
			fs.parentMachine.FollowConnection(gConnect);
		}	
		else
		{
			UnityEngine.Debug.LogWarning("TrainGame: couldn't find flow connector - " + name);	
		}
	}
	
	public override GConnector GetFinalConnection ()
	{
		if(bFailed || Platform.Instance.LocalPlayerPosition.Distance < finish)
		{
			DataVault.Set("train_subtitle", "\"Aaaaargh!\"");
		}
		else
		{
			DataVault.Set("train_subtitle", "\"My Hero!\"");
		}

		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find( r => r.Name == "Subtitle" );
		
		//fire off coroutine to progress past this screen in 2 seconds
		StartCoroutine( ProgressToFinish() );
		
		return gConnect;
	}
	
	IEnumerator ProgressToFinish()
	{
		UnityEngine.Debug.Log("TrainGame: Progressing to finish in 5 seconds");
		//wait for 2s then continue
		yield return new WaitForSeconds(5.0f);
		
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find( r => r.Name == "Finish");
		if(gConnect != null)
		{
			fs.parentMachine.FollowConnection(gConnect);
		}
		else
		{
			UnityEngine.Debug.LogWarning("Train: Couldn't find Finish connector!");	
		}
		

	}
}
