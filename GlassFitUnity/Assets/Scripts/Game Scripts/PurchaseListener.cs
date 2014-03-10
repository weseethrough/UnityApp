using UnityEngine;
using System.Collections;

public class PurchaseListener : MonoBehaviour {
	
	private GestureHelper.OnTap tapHandler = null;
	
	private GestureHelper.OnBack backHandler = null;
	
	// Use this for initialization
	void Start () {
		tapHandler = new GestureHelper.OnTap(() => {
			PurchaseGame();
		});
		
		GestureHelper.onTap +=  tapHandler;
		
		backHandler = new GestureHelper.OnBack(() => {
			GoBack();
		});
		
		GestureHelper.onBack += backHandler;
	}
	
	void GoBack() 
	{
		FlowState.FollowFlowLinkNamed("MenuButton");
	}
	
	void PurchaseGame() {
		if(FlowState.FollowFlowLinkNamed("MenuButton")) {
			Game current = (Game)DataVault.Get("actual_game");
			UnityEngine.Debug.Log("Purchase: Game bought");
			current.Unlock();
		}
	}
	
	void Update() {
		if(Input.GetKeyDown(KeyCode.Escape)) {
			FlowState.FollowFlowLinkNamed("MenuButton");
		}
	}
	
	// Update is called once per frame
	void OnDestroy() {
		GestureHelper.onTap -= tapHandler;
		GestureHelper.onBack -= backHandler;
	}
}
