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
	UIAnchor opponentSpriteAnchor;

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
		opponentProgressBar = GameObject.Find("Progress Bar Opponent").GetComponent<UISlider>();
		if(opponentProgressBar == null)
		{
			log.error("Couldn't find opponent progress bar");
		}
		
		targetDistance = GameBase.getTargetDistance();

		//find opponent object
		opponentObj = GameObject.Find("DavidRealWalk").GetComponent<RYWorldObject>();
		if(opponentObj == null) { log.error("Couldn't find opponent object"); }

		//find sprites
		playerSpriteAnchor = GameObject.Find("Sprite_Player").GetComponent<UIAnchor>();
		opponentSpriteAnchor = GameObject.Find("Sprite_Opponent").GetComponent<UIAnchor>();

		//find the run and start it
		GameBase game = GameObject.FindObjectOfType<GameBase>();
		game.TriggerUserReady();
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
		float opponentDist = opponentObj.getRealWorldPos().z;
		float opponentProgress = opponentDist / targetDistance;
		opponentProgressBar.value = opponentDist / targetDistance;
		
		// Update Sprite positions
		playerSpriteAnchor.relativeOffset.x = playerProgress;
		opponentSpriteAnchor.relativeOffset.x = opponentProgress;
		
		// check for race finished
		if(playerDist > targetDistance)
		{
			//we're done
		}
	}
}


