using UnityEngine;
using System.Collections;

public class TapToBegin : MPChildGestureHandler {
	
	protected override void HandleTap()
	{
		//progress to next screen
		//follow the 'begin' link
		FlowState.FollowFlowLinkNamed("Begin" );
		SoundManager.PlaySound(SoundManager.Sounds.Tap);
	
		//start game, and ensure track is visible
		GameBase game = GameObject.FindObjectOfType(typeof(GameBase)) as GameBase;
		if(game != null)
		{
			game.SetVirtualTrackVisible(true);
			//game.SetReadyToStart(true);
			game.TriggerUserReady();
		}
		else
		{
			UnityEngine.Debug.Log("Couldn't find GameBase instance to unhide virtual track");
		}
	}

}
