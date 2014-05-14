using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Collections.Generic;

[Serializable]
public class MobileInRun : MobilePanel {

	Log log = new Log("MobileInRun");
		
	UISlider playerProgressBar;
	UISlider opponentProgressBar;

	int targetDistance = 0;
	float targetTime = 0;

	RYWorldObject opponentObj;

	RunnerSpriteAnimation playerSpriteAnimation;
	RunnerSpriteAnimation opponentSpriteAnimation;

	UIWidget AheadBehindBG;

	bool bPaused = false;

	bool bPlayerAhead = true;

	public MobileInRun() { }
	public MobileInRun(SerializationInfo info, StreamingContext ctxt)
		: base(info, ctxt)
	{
	}

	/// <summary>
	/// Gets display name of the node, helps with node identification in editor
	/// </summary>
	/// <returns>name of the node</returns>
	public override string GetDisplayName()
	{
		base.GetDisplayName();
		
		GParameter gName = Parameters.Find(r => r.Key == "Name");
		if (gName != null)
		{
			return "MobileInRunPanel: " + gName.Value;
		}
		return "MobileInRunPanel: UnInitialzied";
	}

	// Use this for initialization
	void Start () {
		//put initialisation in enterStart instead
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public override void EnterStart()
	{
		base.EnterStart();

		
		log.info("Starting in game panel");
		
		playerProgressBar = GameObject.Find("Progress Bar Player").GetComponent<UISlider>();
		if(playerProgressBar == null)
		{
			log.error("Couldn't find player progress bar");
		}
		else log.info("Found player progress bar");

		opponentProgressBar = GameObject.Find("Progress Bar Opponent").GetComponent<UISlider>();
		if(opponentProgressBar == null)
		{
			log.error("Couldn't find opponent progress bar");
		}
		else log.info("Found opponent progress bar");
		
		targetTime = GameBase.getTargetTime();
		targetDistance = GameBase.getTargetDistance();

		//log.info("Got target distance");

		//find opponent object
		GameObject opp = GameObject.Find("DavidRealWalk");
		if(opp != null)
		{
			opponentObj = opp.GetComponent<RYWorldObject>();
		}
		if(opponentObj == null) { log.error("Couldn't find opponent object"); }
		else log.info("Found opponent object");

		Platform.Instance.LocalPlayerPosition.Reset();

		//find sprites
		GameObject playerObject = GameObject.Find("Sprite_Player");
		playerSpriteAnimation = playerObject.GetComponent<RunnerSpriteAnimation>();
		playerSpriteAnimation.stationary = true;

		GameObject opponentObject = GameObject.Find("Sprite_Opponent");
		opponentSpriteAnimation = opponentObject.GetComponent<RunnerSpriteAnimation>();
		opponentSpriteAnimation.stationary = true;

		log.info("Found sprites");

		//find the run and start it
		GameBase game = GameObject.FindObjectOfType<GameBase>();
		if(game == null) { log.error("Couldn't find game base"); }
		else
		{
			game.TriggerUserReady();
			log.info("started run");
		}

		DataVault.Set("paused", false);

		//Add flowbuttons to each uiimagebutton
		Component[] buttons = physicalWidgetRoot.GetComponentsInChildren(typeof(UIImageButton), true);
		if (buttons != null && buttons.Length > 0)
		{
			foreach (UIImageButton bScript in buttons)
			{
				FlowButton fb = bScript.GetComponent<FlowButton>();
				if (fb == null)
				{
					fb = bScript.gameObject.AddComponent<FlowButton>();
				}
				
				fb.owner = this;
				fb.name = fb.transform.parent.name;
			}
		}

		GameObject bg = GameObject.Find("BG");
		if(bg != null)
		{
			AheadBehindBG = bg.GetComponent<UIWidget>();
		}
	}
	
	public override void ExitStart ()
	{
		base.ExitStart ();
		
		//stop tracking
		Platform.Instance.LocalPlayerPosition.Reset();
	}
	
	// Update is called once per frame
	void Update () {
		//use stateUpdate()
	}
	
	public override void StateUpdate ()
	{
		base.StateUpdate ();

		// Fill progress bar based on player distance 
		float playerDist = (float)Platform.Instance.LocalPlayerPosition.Distance;

		//reset the goal distance if the player has exceeded the challenge track distance
		if(playerDist > targetDistance)
		{
			targetDistance = Mathf.FloorToInt(playerDist);
			DataVault.Set("finish", targetDistance);
		}

		float playerProgress = playerDist / targetDistance;
		playerProgressBar.value = playerProgress;
		
		// Fill progress bar based on opponent distance
		if(opponentObj == null)
		{
			GameObject opp = GameObject.Find("DavidRealWalk");
			if(opp == null) { /*log.error("Couldn't find opponent object in real world");*/ }
			else
			{
				opponentObj = opp.GetComponent<RYWorldObject>();
				log.info("Found opponent object in state update");
			}
		}

		//update ahead/behind colour

		float opponentDist = 0;
		if(opponentObj != null)
		{ 
			 opponentDist = opponentObj.getRealWorldPos().z;
		}

		//string for HUD / results screen
		string opponentDistString = UnitsHelper.SiDistanceUnitless(opponentDist, "opponent_distance_units");
		DataVault.Set("opponent_distance", opponentDistString);

		//keep track of winner in datavault
		bool bPlayerAheadNow = (playerDist >= opponentDist);
		if( bPlayerAhead != bPlayerAheadNow )
		{
			bPlayerAhead = bPlayerAheadNow;
			DataVault.Set("player_is_ahead", bPlayerAhead);
		}

		float opponentProgress = opponentDist / targetDistance;
		opponentProgressBar.value = opponentProgress;

		if(opponentDist > playerDist)
		{
			DataVault.Set("mobile_aheadBehind_colour", UIColour.red);
		}
		else
		{
			DataVault.Set("mobile_aheadBehind_colour", UIColour.green);
		}

		// Update Sprite positions
		float activeWidth = Screen.width * 0.5f;
		playerSpriteAnimation.transform.localPosition = new Vector3( -activeWidth/2 + playerProgress * activeWidth, playerSpriteAnimation.transform.localPosition.y, 0);
		playerSpriteAnimation.stationary = Platform.Instance.LocalPlayerPosition.Pace < 1.0f || !Platform.Instance.LocalPlayerPosition.IsTracking;

		opponentSpriteAnimation.transform.localPosition = new Vector3( -activeWidth/2 + opponentProgress * activeWidth, playerSpriteAnimation.transform.localPosition.y, 0);
		//no convenient interface to get opponent speed atm, just make it always run for now
		opponentSpriteAnimation.stationary = !Platform.Instance.LocalPlayerPosition.IsTracking;

		// check for race finished
		float time = Platform.Instance.LocalPlayerPosition.Time;
		if(time > targetTime * 1000)
		{
			//calculate average pace for player and opponent
			float elapsedTime = Platform.Instance.LocalPlayerPosition.Time;
			float playerSpeed = playerDist / elapsedTime;
			float playerKmPace = UnitsHelper.SpeedToKmPace(playerSpeed);
			string playerPaceString = UnitsHelper.kmPaceToString(playerKmPace);
			DataVault.Set("player_average_pace", playerPaceString);

			float opponentSpeed = opponentDist / time;
			float opponentKmPace = UnitsHelper.SpeedToKmPace(opponentSpeed);
			string opponentPaceString = UnitsHelper.kmPaceToString(opponentKmPace);
			DataVault.Set("opponent_average_pace", opponentPaceString);

			//we're done
			// load new scene
			AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);

			//progress flow to results
			FlowState.FollowFlowLinkNamed("Finished");
		}
	}

	public override void OnClick (FlowButton button)
	{
		if(!Platform.Instance.LocalPlayerPosition.IsTracking) { return; }

		if(button.name == "Paused" || button.name == "Unpaused")
		{
			//toggle paused-ness
			if(!bPaused)
			{
				Platform.Instance.LocalPlayerPosition.StopTrack();		
				bPaused = true;
				//set in datavault to control visibility of items
				DataVault.Set("paused", true);
				//pause the runners
				playerSpriteAnimation.framesPerSecond = 0;
				opponentSpriteAnimation.framesPerSecond = 0;
			}
			else
			{
				Platform.Instance.LocalPlayerPosition.StartTrack();
				bPaused = false;
				//set in datavault to control visibility of items
				DataVault.Set("paused", false);
				//resume the runners
				playerSpriteAnimation.framesPerSecond = 10;
				opponentSpriteAnimation.framesPerSecond = 10;
			}
		}
		base.OnClick (button);
	}
}


