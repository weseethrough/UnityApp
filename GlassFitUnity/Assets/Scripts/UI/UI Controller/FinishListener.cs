using UnityEngine;
using System.Collections;

public class FinishListener : MonoBehaviour {
	
	
	private bool started = false;
	
	GestureHelper.OnTap tapHandler = null;
	
	void Start() {
		tapHandler = new GestureHelper.OnTap( () => {
			ContinueGame();
			GestureHelper.onTap -= tapHandler;
		});
		GestureHelper.onTap += tapHandler;
	}
	
	void Update() {
		
	}
	
	void ContinueGame() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "ContinueButton");
		UnityEngine.Debug.Log("FinishListener: Finding output");
		if(gConnect != null)
		{
			fs.parentMachine.FollowConnection(gConnect);
			AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
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
