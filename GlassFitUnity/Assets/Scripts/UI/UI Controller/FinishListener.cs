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
		UnityEngine.Debug.Log("FinishListener: Finding output");
		FlowStateBase.FollowFlowLinkNamed("ContinueButton");
		AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
		started = false;
	}
	
	void OnDestroy () {
		
	}
}
