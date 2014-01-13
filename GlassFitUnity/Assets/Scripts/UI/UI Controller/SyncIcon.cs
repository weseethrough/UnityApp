using UnityEngine;
using System.Collections;

public class SyncIcon : MonoBehaviour {
	
	private float rotation = 0f;
	
	private Platform.OnSync syncHandler = null;
	
	// Use this for initialization
	void Start () {
		
		if(Platform.Instance.IsPluggedIn()) {
			Platform.Instance.SyncToServer();
			syncHandler = new Platform.OnSync(() => {
				GoToGame();
			});
			 
			Platform.Instance.onSync += syncHandler;
		} else {
			GoToGame();
		}
	}
	
//	IEnumerator SyncServer() {
//		if(Platform.Instance.IsPluggedIn()) {
//			
//			Platform.Instance.SyncToServer();
//			
//			yield return new WaitForEndOfFrame();
//			
//			GoToGame();
//			
//		} else {
//			GoToGame();
//		}
//	}
	
	void GoToGame() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "GameExit");
			
		if(gConnect != null) {
			fs.parentMachine.FollowConnection(gConnect);
			Platform.Instance.onSync -= syncHandler;
		} else {
			UnityEngine.Debug.Log("SyncIcon: Game exit not found");
		}
	}
	
	// Update is called once per frame
	void Update () {
		rotation -= 360.0f * Time.deltaTime;
		
		transform.rotation = Quaternion.Euler(0, 0, rotation);
	}
}
