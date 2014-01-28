using UnityEngine;
using System.Collections;

public class DeviceControl : MonoBehaviour {

	private float pollTime = 0.0f;
	
	// Use this for initialization
	void Start () {
		DataVault.Set("first_menu", " ");
		
		if(Platform.Instance.Device() != null)
		{
			UnityEngine.Debug.Log("DeviceControl: device obtained");
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConnect = fs.Outputs[0];
			if(gConnect != null)
			{
				UnityEngine.Debug.Log("DeviceControl: connection found, travelling");
				fs.parentMachine.FollowConnection(gConnect);
			} else
			{
				UnityEngine.Debug.Log("DeviceControl: connection not found");
			}
		} else
		{
			UnityEngine.Debug.Log("DeviceControl: device null");
		}
	}
	
	// Update is called once per frame
	void Update () {
		pollTime += Time.deltaTime;
		
		if(pollTime > 5.0f) 
		{
#if UNITY_EDITOR

            pollTime -= 500.0f; //simply shouldn't have to happen again
            FlowState fs = FlowStateMachine.GetCurrentFlowState();
            GConnector gConnect = fs.Outputs.Find(r => r.Name == "MenuExit");
            if (gConnect != null)
            {
                UnityEngine.Debug.Log("DeviceControl: Automatic exit");
                fs.parentMachine.FollowConnection(gConnect);
            }
            else
            {
                UnityEngine.Debug.Log("DeviceControl: Connection not found: MenuExit");
            }
#else
			pollTime -= 5.0f;
			if(Platform.Instance.Device() != null)
			{
				UnityEngine.Debug.Log("DeviceControl: device obtained");
				FlowState fs = FlowStateMachine.GetCurrentFlowState();
				GConnector gConnect = fs.Outputs.Find(r => r.Name == "MenuExit");
				if(gConnect != null)
				{
					UnityEngine.Debug.Log("DeviceControl: connection found, travelling");
					fs.parentMachine.FollowConnection(gConnect);
				} else
				{
					UnityEngine.Debug.Log("DeviceControl: connection not found: MenuExit");
				}
			} else
			{
				UnityEngine.Debug.Log("DeviceControl: device null");
			}
#endif
        }
	}
}
