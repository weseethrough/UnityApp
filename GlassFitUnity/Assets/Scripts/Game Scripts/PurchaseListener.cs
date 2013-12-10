using UnityEngine;
using System.Collections;

public class PurchaseListener : MonoBehaviour {
	
	private GestureHelper.OnTap tapHandler = null;
	
	// Use this for initialization
	void Start () {
		tapHandler = new GestureHelper.OnTap(() => {
			PurchaseGame();
		});
		
		GestureHelper.onTap +=  tapHandler;
	}
	
	void PurchaseGame() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "PurchaseButton");
		
		if(gConnect != null) {
			Game current = (Game)DataVault.Get("actual_game");
			UnityEngine.Debug.Log("Purchase: Game bought");
			current.Unlock();
			fs.parentMachine.FollowConnection(gConnect);
		} else {
			UnityEngine.Debug.Log("Purchase: No connection found, cannot purchase");
		}
	}
	
	// Update is called once per frame
	void OnDestroy() {
		GestureHelper.onTap -= tapHandler;
	}
}
