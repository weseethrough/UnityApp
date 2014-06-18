using UnityEngine;
using System.Collections;
using System;

public class SnackBase : MonoBehaviour {
	
	// Boolean for when the game ends
	protected bool finish = false;
	
	protected GameObject mainCamera;
	
	protected ThirdPersonCamera thirdPerson;
	
	/// <summary>
	/// Virtual function for starting the minigame
	/// </summary>
	public virtual void Begin()	
	{
		mainCamera = GameObject.Find("MainGameCamera");
		if(mainCamera != null)
		{
			thirdPerson = mainCamera.GetComponentInChildren<ThirdPersonCamera>();
		}
	}
	
	// Use this for initialization
	public virtual void Start () {
		
	}
	
	// Update is called once per frame
	public virtual void Update () {
		
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
			if(mainCamera != null)
			{
				if(!mainCamera.activeSelf)
				{
					mainCamera.SetActive(true);
				}
			}
			
			if(thirdPerson != null)
			{
				if(!thirdPerson.enabled)
				{
					thirdPerson.enabled = true;
				}
			}
			run.OnSnackFinished();
            //SetVirtualTrackVisible(true); // FIXME commented out as part of refactor for mobile
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
		string siDistance = UnitsHelper.SiDistanceUnitless(Math.Abs(targetDistance), "target_units");
		// Set the distance
		DataVault.Set("ahead_box", siDistance);
	}
	

	protected void SetTrack(bool visible)
	{
		SnackRun snack = (SnackRun)FindObjectOfType(typeof(SnackRun));
		if(snack != null)
		{
            //SetVirtualTrackVisible(visible); // FIXME commented out as part of refactor for mobile
		}
		else
		{
			UnityEngine.Debug.Log("SnackBase: can't find SnackRun");
		}
	}
	
	protected void SetMainCamera(bool visible)
	{
		if(mainCamera != null)
		{
			mainCamera.SetActive(visible);
		}
		else
		{
			UnityEngine.Debug.Log("SnackBase: camera is null");
		}
	}
	
	protected void SetThirdPerson(bool visible)
	{
		if(thirdPerson != null)
		{
			if(!visible)
			{
				thirdPerson.ForceFirst();
			}
			thirdPerson.enabled = false;
		}
	}
}
