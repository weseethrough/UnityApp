using UnityEngine;
using System.Collections;

public class QuitListener : MonoBehaviour {
	
	GestureHelper.DownSwipe downHandler = null;
	
	GestureHelper.OnTap tapHandler = null;
	
	// Use this for initialization
	void Start () {
		tapHandler = new GestureHelper.OnTap(() => {
			
		});
		
		GestureHelper.onTap += tapHandler;
		
		downHandler = new GestureHelper.DownSwipe(() => {
			
		});
		GestureHelper.onSwipeDown += downHandler;
	}
	
	void ReturnGame() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "GameExit");
		if(gConnect != null) {
			GestureHelper.onSwipeDown -= downHandler;
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
