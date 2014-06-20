using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Collections.Generic;
using RaceYourself;
using RaceYourself.Models;
using Newtonsoft.Json;

[Serializable]
public class MobileInRun : MobilePanel {

	//opponent's pace is never stored and may be needed in other places, will calculate here for now and move later
	float opponentPace = 0;
	float opponentDistance = 0;
	float localTime;

	float playerKmPace;

	GameObject speedBox;
	GameObject positionBox;

	bool changePage = false;

	Log log = new Log("MobileInRun");
	
	UISlider playerProgressBar;
	UISlider opponentProgressBar;

	int targetDistance = 0;
	float targetTime = 0;

	WorldObject opponentObj;

	RunnerSpriteAnimation playerSpriteAnimation;
	RunnerSpriteAnimation opponentSpriteAnimation;

    VoiceFeedbackController voiceFeedbackController;

	UIWidget AheadBehindBG;

	Color currentColor = new Color(1, 1, 1);

	Color behindColor = new Color(254/255f, 0f, 0f);
	Color aheadColor = new Color(47/255f, 203/255f, 0f);

	GameObject aheadBG;
	GameObject aheadDist;
	GameObject aheadStatus;
	GameObject aheadUnits;

	bool bPaused = false;

	bool bPlayerAhead = true;

	UISprite pauseButtonImage = null;

	Track track;

	public MobileInRun() { }
	public MobileInRun(SerializationInfo info, StreamingContext ctxt)
		: base(info, ctxt)
	{
	}

	float elapsedTime = 0;

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
		//Platform.Instance.
		//put initialisation in enterStart instead
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public override void EnterStart()
	{
		base.EnterStart();

		DataVault.Set("paused", false);

		DataVault.Set("second_page", false);

		DataVault.Set("elapsed_time", 0);

		DataVault.Set("player_average_pace", "--:--");

		DataVault.Set("locked", true);

//		speedBox = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "SpeedBox");
//		speedBox.SetActive(false);

//		positionBox = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PositionBox");

		aheadBG = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "AheadBG");
		aheadDist = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "AheadDist");
		aheadStatus = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "AheadStatus");
		aheadUnits = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "AheadUnits");

		DataVault.Set("elapased_time", 0f);

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
		
		//targetTime = GameBase.getTargetTime();
		targetTime = (float)(int)DataVault.Get("finish_time_seconds");
		targetDistance = GameBase.getTargetDistance();

		//log.info("Got target distance");

		Platform.Instance.LocalPlayerPosition.Reset();

		//find sprites
        GameObject playerObject = GameObject.Find("PlayerActor");
		playerSpriteAnimation = playerObject.GetComponentInChildren<RunnerSpriteAnimation>();
		playerSpriteAnimation.stationary = true;

        GameObject opponentObject = GameObject.Find("OpponentActor");
        opponentSpriteAnimation = opponentObject.GetComponentInChildren<RunnerSpriteAnimation>();
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

        // Ahead/behind circle
		GameObject bg = GameObject.Find("BG");
		if(bg != null)
		{
			AheadBehindBG = bg.GetComponent<UIWidget>();
		}

        track = game.selectedTrack;
        Notification challengeNotification = (Notification)DataVault.Get("challenge_notification");

        // initialise voice feedback
        voiceFeedbackController = GameObject.FindObjectOfType<VoiceFeedbackController>();
        voiceFeedbackController.player = Platform.Instance.LocalPlayerPosition;
        voiceFeedbackController.opponent = opponentObj;
        if(track != null) voiceFeedbackController.track = track;

        // analytics for race start
		Hashtable eventProperties = new Hashtable();
		eventProperties.Add("event_name", "start_race");
        if(track != null) eventProperties.Add("track_id", track.trackId.ToString());
        if(challengeNotification != null) eventProperties.Add("challenge_id", challengeNotification.message.challenge_id);
        Platform.Instance.LogAnalyticEvent(JsonConvert.SerializeObject(eventProperties));

		//find the pause button sprite
		GameObject pb = GameObject.Find("PauseButtonImage");
		pauseButtonImage = pb.GetComponent<UISprite>();
		if(pauseButtonImage == null)
		{
			log.error("couldn't find pause button sprite");
		}
	}
	
	public override void ExitStart ()
	{
		base.ExitStart ();
		
		float playerDist = (float)Platform.Instance.LocalPlayerPosition.Distance;
		float opponentDist = opponentObj.getRealWorldPos().z;

		//calculate average pace for player and opponent
		//float elapsedTime = Platform.Instance.LocalPlayerPosition.Time / 1000;
		float playerSpeed = playerDist / elapsedTime;
		float playerKmPace = UnitsHelper.SpeedToKmPace(playerSpeed);
		string playerPaceString = UnitsHelper.kmPaceToString(playerKmPace);
		DataVault.Set("player_average_pace", playerPaceString);

		float opponentSpeed = opponentDist / elapsedTime;
		float opponentKmPace = UnitsHelper.SpeedToKmPace(opponentSpeed);
		string opponentPaceString = UnitsHelper.kmPaceToString(opponentKmPace);
		DataVault.Set("opponent_average_pace", opponentPaceString);

		//no longer than original target
		//elapsedTime = Mathf.Min(elapsedTime, elapsedTime); //some extra special code here!

		string timeString = UnitsHelper.TimestampMMSSfromMillis((long)elapsedTime * 1000);
		DataVault.Set("finish_time", timeString);

		int minutes = Mathf.FloorToInt( elapsedTime / 60f );
		string timeMinutes = minutes.ToString();
		DataVault.Set("finish_time_minutes", timeMinutes);

        Track currentTrack = Platform.Instance.LocalPlayerPosition.StopTrack();
        Notification challengeNotification = (Notification)DataVault.Get("challenge_notification");
        Device device = Platform.Instance.Device();
		string duration = (string)DataVault.Get("duration");
        bool result = Convert.ToBoolean(DataVault.Get("player_is_ahead"));
        bool challengeComplete = false;

		if(timeMinutes.Equals(duration)) {
			// log attempt
            if(currentTrack != null && challengeNotification != null && device != null) {
                challengeComplete = true;
                UnityEngine.Debug.Log("MobileInRun: Challenge ID is " + challengeNotification.message.challenge_id);
				UnityEngine.Debug.Log("MobileInRun: Device ID is " + device.id);
                UnityEngine.Debug.Log("MobileInRun: Track ID is " + currentTrack.trackId);

				Platform.Instance.QueueAction(string.Format(@"{{'action': 'challenge_attempt', 
												'challenge_id': {0}, 
												'track_id' : [
													{1}, {2}
												]
									}}", challengeNotification.message.challenge_id, device.id, currentTrack.trackId).Replace("'", "\""));
			} else
			{
				UnityEngine.Debug.LogError("MobileInRun: No track or notification!");
			}
		}

        // analytics for end of race
        Hashtable eventProperties = new Hashtable();
        eventProperties.Add("event_name", "end_race");
        eventProperties.Add("result", result ? "win" : "loss");
        if (track != null) eventProperties.Add("track_id", track.trackId.ToString());
        if (challengeNotification != null) eventProperties.Add("challenge_id", challengeNotification.message.challenge_id);
        if (challengeNotification != null) eventProperties.Add("challenge_complete", challengeComplete);
        if (challengeNotification != null) eventProperties.Add("challenge_sender", challengeNotification.message.from);
        if (challengeNotification != null) eventProperties.Add("challenge_type", challengeNotification.message.challenge_type);
        Platform.Instance.LogAnalyticEvent(JsonConvert.SerializeObject(eventProperties));

		// load new scene
		AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);



		//stop tracking
		Platform.Instance.LocalPlayerPosition.Reset();
	}
	
	public override void StateUpdate ()
	{
		base.StateUpdate ();

		if(Platform.Instance.LocalPlayerPosition.IsTracking)
		{
			//update elapsed time. Assuming this update ultimately comes from a per-frame update.
			elapsedTime += Time.deltaTime;

			//set in data vault
			DataVault.Set("elapased_time", elapsedTime);
		}

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
            GameObject opp = GameObject.Find("PlayerActor");
			if(opp == null) { /*log.error("Couldn't find opponent object in real world");*/ }
			else
			{
				opponentObj = opp.GetComponent<WorldObject>();
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
			DataVault.Set("encourage_text", "SPEED UP");
			if(currentColor != behindColor)
			{
				currentColor = behindColor;
				aheadBG.GetComponent<UISprite>().color = behindColor;
				aheadDist.GetComponent<UILabel>().color = behindColor;
				aheadStatus.GetComponent<UILabel>().color = behindColor;
				aheadUnits.GetComponent<UILabel>().color = behindColor;
			}
		}
		else
		{
			DataVault.Set("mobile_aheadBehind_colour", UIColour.green);
			DataVault.Set("encourage_text", "WINNING PACE");
			if(currentColor != aheadColor)
			{
				currentColor = aheadColor;
				aheadBG.GetComponent<UISprite>().color = aheadColor;
				aheadDist.GetComponent<UILabel>().color = aheadColor;
				aheadStatus.GetComponent<UILabel>().color = aheadColor;
				aheadUnits.GetComponent<UILabel>().color = aheadColor;
			}
		}

		// Update Sprite positions
		float activeWidth = Screen.width * 0.5f;
//		playerSpriteAnimation.transform.localPosition = new Vector3( -activeWidth/2 + playerProgress * activeWidth, playerSpriteAnimation.transform.localPosition.y, 0);
		playerSpriteAnimation.stationary = Platform.Instance.LocalPlayerPosition.Pace < 1.0f || !Platform.Instance.LocalPlayerPosition.IsTracking;
		playerSpriteAnimation.framesPerSecond =Mathf.Clamp((int)(10 *( Platform.Instance.LocalPlayerPosition.Pace/1.5)),10,50);

		opponentDistance =  opponentDist - opponentDistance;

		opponentPace = opponentDistance/(elapsedTime - localTime);

		//Debug.LogWarning("pace is" + oppoenentsDistance +" divded by this " + localTime + " equlalling this " + opponentsPace);

		opponentPace = opponentDistance/(elapsedTime - localTime);
		//Debug.Log("pace is" + oppoenentsDistance +" divded by this " + localTime + " equlalling this " + opponentsPace);
//		opponentSpriteAnimation.transform.localPosition = new Vector3( -activeWidth/2 + opponentProgress * activeWidth, opponentSpriteAnimation.transform.localPosition.y, 0);

		//DataVault.Set("ahead_box", Mathf.Abs(playerDist-opponentDist));
		opponentSpriteAnimation.framesPerSecond = Mathf.Clamp((int)(70 *( opponentPace/3)),10,50);

		//no convenient interface to get opponent speed atm, just make it always run for now
		opponentSpriteAnimation.stationary = !Platform.Instance.LocalPlayerPosition.IsTracking || opponentPace < 1f;

		localTime = elapsedTime;
		// check for race finished
		float time = elapsedTime;

		if(time > targetTime)
		{
			//we're done

			//progress flow to results
			FlowState.FollowFlowLinkNamed("Finished");
		}
	}

	public override void OnClick (FlowButton button)
	{

		if(button.name == "Paused" || button.name == "Unpaused")
		{
			//toggle paused-ness
			bool isPaused = Convert.ToBoolean(DataVault.Get("paused"));
			if(!isPaused && Platform.Instance.LocalPlayerPosition.IsTracking)
			{
				Platform.Instance.LocalPlayerPosition.StopTrack();		
//				bPaused = true;
				//set in datavault to control visibility of items
				DataVault.Set("paused", true);
				//pause the runners
				Time.timeScale = 0.0f;
				//playerSpriteAnimation.framesPerSecond = 0;
				//opponentSpriteAnimation.framesPerSecond = 0;
//				pauseButtonImage.spriteName = "mobile_resume";
			}
			else
			{
				Platform.Instance.LocalPlayerPosition.StartTrack();
//				bPaused = false;
				//set in datavault to control visibility of items
				DataVault.Set("paused", false);
				Time.timeScale = 1.0f;
				//resume the runners
				//playerSpriteAnimation.framesPerSecond = 10;
				//opponentSpriteAnimation.framesPerSecond = 10;
//				pauseButtonImage.spriteName = "paused";
			}
		} 
		else if(button.name == "Locked")
		{
			DataVault.Set("locked", false);
		}
		else if(button.name == "Unlocked")
		{
			DataVault.Set("locked", true);
		}
		else if(button.name == "ChangePageBtn")
		{
			changePage = !changePage;
			DataVault.Set("second_page", changePage);
		}
		base.OnClick (button);
	}
}


