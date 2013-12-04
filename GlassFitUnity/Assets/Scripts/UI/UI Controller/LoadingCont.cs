using UnityEngine;
using System.Collections;

public class LoadingCont : MonoBehaviour {
	
	private float rotate = 0;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		rotate += 10 * Time.deltaTime;
		
//		FlowState fs = FlowStateMachine.GetCurrentFlowState();
//		GConnector gConect = fs.Outputs.Find(r => r.Name == "completeExit");
//		if(gConect != null) {
//			fs.parentMachine.FollowConnection(gConect);
//		} else {
//			UnityEngine.Debug.Log("Game: No connection found!");
//		}
		
		transform.rotation = Quaternion.Euler(0, 0, rotate);
	}
}
