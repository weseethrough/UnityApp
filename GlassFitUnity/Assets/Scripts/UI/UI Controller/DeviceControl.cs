using UnityEngine;
using System.Collections;

public class DeviceControl : MonoBehaviour {

	private float pollTime = 0.0f;
	
	// Use this for initialization
	void Start () {
		DataVault.Set("first_menu", " ");
		
		//set default strings for this screen.
		DataVault.Set("register_title", "Initialising");
		DataVault.Set("register_body", "Please Wait");
		
		UpdateStatus();
	}
	
	/// <summary>
	/// Updates the status.
	/// Check if we have wifi/device, and either update the strings or proceed to next menu
	/// </summary>
	protected void UpdateStatus()
	{
		if(Platform.Instance.HasWifi())
		{
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
				SetStringsNoDevice();
				UnityEngine.Debug.Log("DeviceControl: device null");
			}
		}
		else
		{
			SetStringsNoInternet();
		}
		
	}
	
	protected void SetStringsNoInternet()
	{
		DataVault.Set("register_title", "Please Connect to the Internet");
		DataVault.Set("register_body", "Welcome to Race Yourself!\n\nBefore first using this software you must be connected to the internet.");
	}
	
	protected void SetStringsNoDevice()
	{
		DataVault.Set("register_title", "Please Sign Up");
		DataVault.Set("register_body", "You must register on the RaceYourself website before running this build\n\nPlease visit:\nauth.raceyourself.com/users/sign_up");
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
			
			UpdateStatus();
//			if(Platform.Instance.Device() != null)
//			{
//				UnityEngine.Debug.Log("DeviceControl: device obtained");
//				FlowState fs = FlowStateMachine.GetCurrentFlowState();
//				GConnector gConnect = fs.Outputs.Find(r => r.Name == "MenuExit");
//				if(gConnect != null)
//				{
//					UnityEngine.Debug.Log("DeviceControl: connection found, travelling");
//					fs.parentMachine.FollowConnection(gConnect);
//				} else
//				{
//					UnityEngine.Debug.Log("DeviceControl: connection not found: MenuExit");
//				}
//			} else
//			{
//				UnityEngine.Debug.Log("DeviceControl: device null");
//			}
#endif
        }
	}
}
