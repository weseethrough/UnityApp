using UnityEngine;
using System.Collections;

public class DeleteController : MonoBehaviour {
	
	private GestureHelper.OnBack backHandler;
	private GestureHelper.OnTap tapHandler;
	
	// Use this for initialization
	void Start () {
		tapHandler = new GestureHelper.OnTap(() => {
			Platform.Instance.ResetGames();
			FlowStateBase.FollowFlowLinkNamed("gameExit");
		});
		
		GestureHelper.onTap += tapHandler;
		
		backHandler = new GestureHelper.OnBack(() => {
			ReturnGame();
		});
		
		GestureHelper.onBack += backHandler;
	}
	
	void ReturnGame() {
		FlowStateBase.FollowFlowLinkNamed("returnExit");
	}
	
	// Update is called once per frame
	void OnDisable() {
		GestureHelper.onTap -= tapHandler;
		GestureHelper.onBack -= backHandler;
	}
}
