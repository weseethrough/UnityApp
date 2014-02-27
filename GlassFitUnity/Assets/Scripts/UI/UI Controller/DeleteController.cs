using UnityEngine;
using System.Collections;

public class DeleteController : MonoBehaviour {
	
	private GestureHelper.OnBack backHandler;
	private GestureHelper.OnTap tapHandler;
	
	// Use this for initialization
	void Start () {
		tapHandler = new GestureHelper.OnTap(() => {
			Platform.Instance.ResetGames();
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConnect = fs.Outputs.Find(r => r.Name == "gameExit");
			if(gConnect != null) 
			{
				fs.parentMachine.FollowConnection(gConnect);	
			} else {
				UnityEngine.Debug.Log("DeleteController: Error finding game exit");
			}
		});
		
		GestureHelper.onTap += tapHandler;
		
		backHandler = new GestureHelper.OnBack(() => {
			ReturnGame();
		});
		
		GestureHelper.onBack += backHandler;
	}
	
	void ReturnGame() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "returnExit");
		
		if(gConnect != null) 
		{
			fs.parentMachine.FollowConnection(gConnect);	
		} else {
			UnityEngine.Debug.Log("DeleteController: Error finding return exit");
		}
	}
	
	// Update is called once per frame
	void OnDisable() {
		GestureHelper.onTap -= tapHandler;
		GestureHelper.onBack -= backHandler;
	}
}
