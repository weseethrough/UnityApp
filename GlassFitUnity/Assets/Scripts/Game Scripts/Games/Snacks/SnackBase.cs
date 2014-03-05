using UnityEngine;
using System.Collections;
using System;

public class SnackBase : MonoBehaviour {
	
	// Boolean for when the game ends
	protected bool finish = false;
	
	/// <summary>
	/// Virtual function for starting the minigame
	/// </summary>
	public virtual void Begin()	
	{
		
	}
	
	// Use this for initialization
	protected virtual void Start () {
		
	}
	
	// Update is called once per frame
	protected virtual void Update () {
		
	}
	
	/// <summary>
	/// Finishes the game.
	/// </summary>
	public virtual void Finish()
	{
		// Call the function in the main game
		SnackRun run = (SnackRun)FindObjectOfType(typeof(SnackRun));
		if(run)
		{
			run.OnSnackFinished();
			run.SetVirtualTrackVisible(true);
		}
		else
		{
			UnityEngine.Debug.Log("SnackBase: not found SnackRun");
		}
		// Destroy the object
		Destroy(transform.gameObject);
	}
	
	/// <summary>
	/// Coroutine to display a banner for a certain amount of time.
	/// </summary>
	/// <returns>
	/// N/A
	/// </returns>
	protected IEnumerator ShowBanner(float waitTime)
	{
		// Try to find the exit for the banner
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "DeathExit");
		if(gConnect != null)
		{
			// Follow the connection
			fs.parentMachine.FollowConnection(gConnect);
			// Wait for 3 seconds
			yield return new WaitForSeconds(waitTime);
			// Return to the game
			fs = FlowStateMachine.GetCurrentFlowState();
			fs.parentMachine.FollowBack();
			// Finish the game if necessary
			if(finish) {
				Finish();
			}	
		}
		else
		{
			UnityEngine.Debug.Log("SnackBase: can't find exit - DeathExit");
		}
	}
	
	/// <summary>
	/// Updates the ahead/behind box.
	/// </summary>
	/// <param name='targetDistance'>
	/// The target's distance.
	/// </param>
	protected void UpdateAhead(double targetDistance) {

		// Set the text and colour based on the target's distance
		if (targetDistance > 0) {
			DataVault.Set("distance_position", "BEHIND");
            DataVault.Set("ahead_col_box", UIColour.red);
		} else {
			DataVault.Set("distance_position", "AHEAD");
            DataVault.Set("ahead_col_box", UIColour.green);
		}
		// Set the units
		string siDistance = SiDistanceUnitless(Math.Abs(targetDistance), "target_units");
		// Set the distance
		DataVault.Set("ahead_box", siDistance);
	}
	
	/// <summary>
	/// Gets the distance as a string and converts it to relevant units.
	/// </summary>
	/// <returns>
	/// The unitless distance.
	/// </returns>
	/// <param name='meters'>
	/// Meters to convert.
	/// </param>
	/// <param name='units'>
	/// Units name to save.
	/// </param>
	protected string SiDistanceUnitless(double meters, string units) {
		string postfix = "m";
		string final;
		float value = (float)meters;
		if (value > 1000) {
			value = value/1000;
			postfix = "km";
			if(value >= 10) {
				final = value.ToString("f1");
			} else {
				final = value.ToString("f2");
			}
		}
		else
		{
			final = value.ToString("f0");
		}
		//set the units string for the HUD
		DataVault.Set(units, postfix);
		return final;
	}
	
	protected void SetTrack(bool visible)
	{
		SnackRun snack = (SnackRun)FindObjectOfType(typeof(SnackRun));
		if(snack != null)
		{
			snack.SetVirtualTrackVisible(visible);
		}
		else
		{
			UnityEngine.Debug.Log("BoulderSnack: can't find SnackRun");
		}
	}
}
