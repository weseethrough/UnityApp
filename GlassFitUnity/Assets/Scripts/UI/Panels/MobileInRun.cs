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

	float targetDistance = 0;

	RYWorldObject opponentObj;

	UIAnchor playerSpriteAnchor;
	UISpriteAnimation playerSpriteAnimation;
	UIAnchor opponentSpriteAnchor;
	UISpriteAnimation opponentSpriteAnimation;

	bool bPaused = false;

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
		
		targetDistance = GameBase.getTargetDistance();

		log.info("Got target distance");

		//find opponent object
		GameObject opp = GameObject.Find("DavidRealWalk");
		if(opp != null)
		{
			opponentObj = opp.GetComponent<RYWorldObject>();
		}
		if(opponentObj == null) { log.error("Couldn't find opponent object"); }
		else log.info("Found opponent object");


		//find sprites
		GameObject playerObject = GameObject.Find("Sprite_Player");
		playerSpriteAnchor = playerObject.GetComponent<UIAnchor>();
		playerSpriteAnimation = playerObject.GetComponent<UISpriteAnimation>();
		GameObject opponentObject = GameObject.Find("Sprite_Opponent");
		opponentSpriteAnchor = opponentObject.GetComponent<UIAnchor>();
		opponentSpriteAnimation = opponentObject.GetComponent<UISpriteAnimation>();

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
	}
	
	public override void ExitStart ()
	{
		base.ExitStart ();
		
		//stop tracking
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


		float opponentDist = 0;
		if(opponentObj != null)
		{ 
			 opponentDist = opponentObj.getRealWorldPos().z;
		}

		float opponentProgress = opponentDist / targetDistance;
		opponentProgressBar.value = opponentDist / targetDistance;
		
		// Update Sprite positions
		playerSpriteAnchor.relativeOffset.x = playerProgress;
		opponentSpriteAnchor.relativeOffset.x = opponentProgress;
		
		// check for race finished
		if(playerDist > targetDistance)
		{
			//we're done
			// load new scene
			AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);

			//progress flow to results
			FlowState.FollowFlowLinkNamed("Finished");

		}
	}

	public override void OnClick (FlowButton button)
	{
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


