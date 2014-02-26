using UnityEngine;
using System.Collections;

public class TapToBegin : MPChildGestureHandler {
	
	protected override void HandleTap()
	{
		//progress to next screen
		//follow the 'begin' link
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find( r => r.Name == "Begin" );
		if(gConnect != null)
		{
			SoundManager.PlaySound(SoundManager.Sounds.Tap);
			fs.parentMachine.FollowConnection(gConnect);
		}
	
		//start game, and ensure track is visible
		GameBase game = GameObject.FindObjectOfType(typeof(GameBase)) as GameBase;
		if(game != null)
		{
			game.SetVirtualTrackVisible(true);
			game.SetReadyToStart(true);
		}
		else
		{
			UnityEngine.Debug.Log("Couldn't find GameBase instance to unhide virtual track");
		}
	}

}
