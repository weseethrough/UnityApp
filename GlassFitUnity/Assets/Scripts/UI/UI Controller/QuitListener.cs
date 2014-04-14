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
		FlowStateBase.FollowFlowLinkNamed("GameExit");
	}
	
	void QuitGame() {
		FlowStateBase.FollowFlowLinkNamed("MenuExit");
		GestureHelper.onTap -= tapHandler;
		AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
	}
}
