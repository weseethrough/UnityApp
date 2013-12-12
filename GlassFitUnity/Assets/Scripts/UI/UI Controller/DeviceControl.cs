using UnityEngine;
using System.Collections;

public class DeviceControl : MonoBehaviour {

	private float pollTime = 0.0f;
	
	// Use this for initialization
	void Start () {
		if(Platform.Instance.Device() != null)
		{
			UnityEngine.Debug.Log("ButtonFunctionCollection: device obtained");
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConnect = fs.Outputs.Find(r => r.Name == "MenuExit");
			if(gConnect != null)
			{
				UnityEngine.Debug.Log("ButtonFunctionCollection: connection found, travelling");
				fs.parentMachine.FollowConnection(gConnect);
			} else
			{
				UnityEngine.Debug.Log("ButtonFunctionCollection: connection not found: MenuExit");
			}
		} else
		{
			UnityEngine.Debug.Log("ButtonFunctionCollection: device null");
		}
	}
	
	// Update is called once per frame
	void Update () {
		pollTime += Time.deltaTime;
		
		if(pollTime > 5.0f) 
		{
			pollTime -= 5.0f;
			if(Platform.Instance.Device() != null)
			{
				UnityEngine.Debug.Log("ButtonFunctionCollection: device obtained");
				FlowState fs = FlowStateMachine.GetCurrentFlowState();
				GConnector gConnect = fs.Outputs.Find(r => r.Name == "MenuExit");
				if(gConnect != null)
				{
					UnityEngine.Debug.Log("ButtonFunctionCollection: connection found, travelling");
					fs.parentMachine.FollowConnection(gConnect);
				} else
				{
					UnityEngine.Debug.Log("ButtonFunctionCollection: connection not found: MenuExit");
				}
			} else
			{
				UnityEngine.Debug.Log("ButtonFunctionCollection: device null");
			}
		}
	}
}
