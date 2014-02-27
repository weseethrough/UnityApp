using UnityEngine;
using System.Collections;

public class QuitListener : MonoBehaviour {
	
	GestureHelper.OnBack backHandler = null;
	
	GestureHelper.OnTap tapHandler = null;
	
	// Use this for initialization
	void Start () {
		tapHandler = new GestureHelper.OnTap(() => {
			
		});
		
		GestureHelper.onTap += tapHandler;
		
		backHandler = new GestureHelper.OnBack(() => {
			
		});
		GestureHelper.onBack += backHandler;
	}
	
	void ReturnGame() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "GameExit");
		if(gConnect != null) {
			GestureHelper.onBack -= backHandler;
			fs.parentMachine.FollowConnection(gConnect);
			
		} else {
			UnityEngine.Debug.Log("QuitListener: Error finding output - GameExit");
		}
	}
	
	void QuitGame() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "MenuExit");
		if(gConnect != null) {
			GestureHelper.onTap -= tapHandler;
			fs.parentMachine.FollowConnection(gConnect);
			AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
		} else {
			UnityEngine.Debug.Log("QuitListener: Error finding output - MenuExit");
		}
	}
}
