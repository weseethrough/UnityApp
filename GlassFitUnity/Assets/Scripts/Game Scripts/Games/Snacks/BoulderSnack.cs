using UnityEngine;
using System.Collections;

public class BoulderSnack : SnackBase {
	
	BoulderController boulder;
	
	public override void Begin ()
	{
		base.Begin ();
		
		if(boulder != null)
		{
			boulder.enabled = true;
		}
		else
		{
			UnityEngine.Debug.Log("BoulderSnack: can't enable boulder - is null!"); 
		}
	}
	
	// Use this for initialization
	void Start () {
		base.Start();
		boulder = GetComponent<BoulderController>();
	}
	
	IEnumerator Finish()
	{
		yield return new WaitForSeconds(2.0f);
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		fs.parentMachine.FollowBack();
		Destroy(transform.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		if(boulder != null)
		{
			if(boulder.GetDistanceBehindTarget() > 0.0)
			{
				
				FlowState fs = FlowStateMachine.GetCurrentFlowState();
				GConnector gConnect = fs.Outputs.Find(r => r.Name == "DeathExit");
				if(gConnect != null)
				{
					fs.parentMachine.FollowConnection(gConnect);
					StartCoroutine(Finish());
				}
				else
				{
					UnityEngine.Debug.Log("BoulderSnack: can't find exit!");
				}
				
			}
		}
		else
		{
			UnityEngine.Debug.Log("BoulderSnack: can't check boulder distance - is null");
		}
	}
}
