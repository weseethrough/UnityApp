using UnityEngine;
using System.Collections;
using System;

public class SnackBase : MonoBehaviour {

	public virtual void Begin()	
	{
		
	}
	
	// Use this for initialization
	protected virtual void Start () {
		GraphComponent gc = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
       	gc.GoToFlow("TestSnackFlow");

	}
	
	// Update is called once per frame
	protected virtual void Update () {
		if(Input.GetKeyDown(KeyCode.Space))
		{
			Begin();
		}
	}
	
	protected virtual void Finish()
	{
		SnackRun run = (SnackRun)FindObjectOfType(typeof(SnackRun));
		if(run)
		{
			run.OnSnackFinished();
		}
		else
		{
			UnityEngine.Debug.Log("BoulderSnack: not found SnackRun");
		}
		Destroy(transform.gameObject);
	}
	
	protected IEnumerator ShowBanner()
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "DeathExit");
		if(gConnect != null)
		{
			fs.parentMachine.FollowConnection(gConnect);
			yield return new WaitForSeconds(3.0f);
			fs = FlowStateMachine.GetCurrentFlowState();
			fs.parentMachine.FollowBack();
			Finish();
		}
		else
		{
			UnityEngine.Debug.Log("SnackBase: can't find exit - DeathExit");
		}
	}
	
	protected void UpdateAhead(double targetDistance) {

		if (targetDistance > 0) {
			DataVault.Set("distance_position", "BEHIND");
			//DataVault.Set("ahead_col_header", "D20000FF");
            DataVault.Set("ahead_col_box", "E5312FFF");
		} else {
			DataVault.Set("distance_position", "AHEAD");
            DataVault.Set("ahead_col_box", "009540FF");
			//DataVault.Set("ahead_col_header", "19D200FF");
		}
		//UnityEngine.Debug.Log("GameBase: distance behind is " + GetDistBehindForHud().ToString());
		string siDistance = SiDistanceUnitless(Math.Abs(targetDistance), "target_units");
		//UnityEngine.Debug.Log("GameBase: setting target distance to: " + siDistance);
		DataVault.Set("ahead_box", siDistance);
	}
	
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
}
