using UnityEngine;
using System.Collections;

public class FinishListener : MonoBehaviour {
	
	
	private bool started = false;
	
	void Update() {
		
	}
	
	void ContinueGame() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "ContinueButton");
		UnityEngine.Debug.Log("FinishListener: Finding output");
		if(gConnect != null)
		{
			fs.parentMachine.FollowConnection(gConnect);
			started = false;
		}
		else
		{
			UnityEngine.Debug.Log("FinishListener: Couldn't find connection - continue button");
		}
	}
	
	void OnDestroy () {
		
	}
}
