using UnityEngine;
using System.Collections;

/// <summary>
/// Path point which shows a help subtitle.
/// Shows the specified subtitle for the given number of seconds, pausing the game while it is up.
/// </summary>
public class PathPoint_ShowHelpSubtitle : PathPoint {
	
	public string subString = null;
	public float subTime = 2.0f;
	
	public override void OnArrival ()
	{
		base.OnArrival();
		StartCoroutine(DoSubtitleSequence());
		UnityEngine.Debug.Log("CamPath: Reached subtitle path point");
	}

	IEnumerator DoSubtitleSequence()
	{
		UnityEngine.Debug.Log("CamPath: Reached subtitle path point - in coroutine");
		
		//pause game
		Time.timeScale = 0.0f;
		
		//set subtitle string
		if(subString != null)
		{
			DataVault.Set("train_subtitle", subString);
		}
		else
		{
			DataVault.Set("train_subtitle", "ERROR: String not set");
			UnityEngine.Debug.LogError("CameraPath: String not specified for subtitle");
		}
		
		//progress flow to subtitle message
		FlowStateBase.FollowFlowLinkNamed("Subtitle");
		
		//wait 2s
		//yield return new WaitForSeconds(subTime);
		//WaitforSeconds depends on timescale, so need to do this a different way.
		System.DateTime continueTime = System.DateTime.Now.AddSeconds(subTime);
		while(System.DateTime.Now < continueTime)
		{
			yield return null;	
		}
		
		//progress flow back to Empty screen
		FlowStateBase.FollowFlowLinkNamed("ToBlank");
		
		//unpause
		Time.timeScale = 1.0f;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
