using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RaceYourself.Models;

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
		if(Platform.Instance.Device() != null)
		{
			UnityEngine.Debug.Log("DeviceControl: device obtained");
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConnect;
			List<Game> gamesList = Platform.Instance.GetGames();
			int gameCount = 0;
			Game chosenGame = null;
			for(int i=0; i<gamesList.Count; i++) {
				if(gamesList[i].type != "N/A") {
					gameCount++;
					chosenGame = gamesList[i];
				}
			}
			if(gameCount == 1) {
				DataVault.Set("is_testing", true);
				gConnect = fs.Outputs.Find(r => r.Name == "GameExit");
				switch(chosenGame.type) {
				case "Snack":
					DataVault.Set ("race_type", "snack");
					DataVault.Set("finish", 1000);
					break;
				case "Race":
					DataVault.Set ("race_type", "race");
					DataVault.Set("finish", 1000);
					break;
				case "Challenge":
					DataVault.Set ("race_type", "challenge");
					gConnect = fs.Outputs.Find(r => r.Name == "ChallengeExit");
					break;
				default:
					UnityEngine.Debug.Log("DeviceControl: game type not recognise - it says " + chosenGame.type);
					DataVault.Set("is_testing", false);
					gConnect = fs.Outputs.Find(r => r.Name == "MenuExit");
					break;
				}
			} else {
				DataVault.Set("is_testing", false);
				gConnect = fs.Outputs.Find(r => r.Name == "MenuExit");
			}

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
			
			//we don't have a device. If this is because we don't have internet, set the appropriate string
			if(!Platform.Instance.HasInternet())
			{
				SetStringsNoInternet();
			}
			else
			{
				//we do have internet, so let's deduce that we need to register.
				SetStringsNoDevice();
			}
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
			FlowState.FollowFlowLinkNamed("MenuExit");
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
