using UnityEngine;
using System.Collections;

public class ResetGyrosScreenController : MPChildGestureHandler {
	
	// Use this for initialization
	public override void Start () {
		base.Start();
		
		//ensure the prompt is shown to begin with
		DataVault.Set("showResetGyroPrompt", true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	protected void OnGyrosReset() {
		if( !IsInFocus() )
		{
			return;
		}
		
		GameBase game = GameObject.FindObjectOfType(typeof(GameBase)) as GameBase;
		if(game != null)
		{
			game.SetVirtualTrackVisible(true);
			StartCoroutine(FollowLink());
		}
		else
		{
			UnityEngine.Debug.Log("Couldn't find GameBase instance to unhide virtual track");
		}
	}
	
	IEnumerator FollowLink() {
		//immediately hide the text
		DataVault.Set("showResetGyroPrompt", false);
		
		//shuffle the progress bar along midway through the delay
		yield return new WaitForSeconds(0.8f);
				
		//progress to next screen
		//follow the 'begin' link
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find( r => r.Name == "Begin" );
		if(gConnect != null)
		{
			fs.parentMachine.FollowConnection(gConnect);
		}
		
		//start the game
		GameBase game = GameObject.FindObjectOfType(typeof(GameBase)) as GameBase;
		if(game != null)
		{
			game.SetReadyToStart(true);
		}
	}
	
}
