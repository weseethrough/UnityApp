using UnityEngine;
using System;
using System.Collections;
using SimpleJSON;

using RaceYourself.Models;
using Newtonsoft.Json;

public class LoadingScreen : MonoBehaviour {
	
	private float rotation = 0f;
	
	AsyncOperation async;
	
	private string levelName;
	protected string raceType;
	
	private UISlider slider;
	
	// Use this for initialization
	void Start () {
		raceType = (string)DataVault.Get("race_type");
		Track track = (Track)DataVault.Get("current_track");
		switch(raceType) {
		case "race":
			//if we have a track, load Race Mode, otherwise, load FirstRun. N.B. the menu flow will be different, so it isn't exactly the same FirstRun experience
			DataVault.Set("custom_redirection_point", "GameIntroExit");
			if(track != null)
			{
				levelName = "Race Mode";
			}
			else
			{
				levelName = "FirstRun";
			}
			break;
		
		case "snack":
			levelName = "SnackRun";
			DataVault.Set("custom_redirection_point", "GameIntroExit");
			break;
			
		case "challenge":
			levelName = "Challenge Mode";
			DataVault.Set("custom_redirection_point", "GameIntroExit");
			break;
			
		case "pursuit":
			levelName = "Pursuit Mode";
			break;
			
		case "tutorial":
			levelName = "FirstRun";
			DataVault.Set("custom_redirection_point", "TutorialIntroExit");
			break;
			
		case "trainRescue":
			levelName = "TrainRescue";
			DataVault.Set("custom_redirection_point", "TrainIntroExit");
			break;
			
		default:
			UnityEngine.Debug.Log("LoadingScreen: ERROR: Unknown race_type: " + raceType);
			break;
		}
		
		if (Platform.Instance.IsDisplayRemote()) {
			//send bluetooth message to load the mode.
            JSONClass json = new JSONClass();
			json.Add("action", "LoadLevelAsync");
			
			JSONClass data = new JSONClass();
			if (track != null) data.Add("current_track", JsonConvert.SerializeObject(track));
			else data.Add("current_track", null);
			data.Add("race_type", raceType);
			if (DataVault.Get("type") != null) data.Add("type", DataVault.Get("type") as string);
			if (DataVault.Get("finish") != null) data.Add("finish", (int)DataVault.Get("finish"));
			if (DataVault.Get("lower_finish") != null) data.Add("lower_finish", (int)DataVault.Get("lower_finish"));
			if (DataVault.Get("challenger") != null) data.Add("challenger", DataVault.Get("challenger") as string);
			if (DataVault.Get("current_challenge_notification") != null) {
				data.Add("current_challenge_notification", JsonConvert.SerializeObject(DataVault.Get("current_challenge_notification") as ChallengeNotification));
			}
			string currentGame = (string)DataVault.Get("current_game_id");
			UnityEngine.Debug.Log("Triggering load: game id = " + currentGame);
			if (DataVault.Get("current_game_id") != null) data.Add("current_game_id", DataVault.Get("current_game_id") as string);
			
			json.Add("data", data);
			Platform.Instance.BluetoothBroadcast(json.ToString());
			MessageWidget.AddMessage("Bluetooth", "Started game on Glass", "settings");
			
			
			UnityEngine.Debug.Log("Loading game: " + raceType);
			// Return to menu

			string exitName = "MenuExit";
			//for snack run, go to the snack remote control meu
			if(raceType == "snack")
			{
				exitName = "SnackRemote";
			}
			FlowStateBase.FollowFlowLinkNamed(exitName);
		}
		
		slider = GetComponentInChildren<UISlider>();
	
		if(slider != null)
		{
			slider.Set(0, false);
		}
		

		StartCoroutine("LoadLevel");

	}
	
	IEnumerator LoadLevel() {
		async = Application.LoadLevelAsync(levelName);
		
		yield return async;
	}
	
	// Update is called once per frame
	void Update () {
		rotation -= 360f * Time.deltaTime;
		
		//transform.rotation = Quaternion.Euler(0, 0, rotation);
		
		if(async != null) {
			float progress = async.progress * 100f;
			if(slider != null) {
				slider.Set(progress / 100f, false);
			}
			UnityEngine.Debug.Log("LoadingScreen: Loading - " + progress.ToString("f0") + "%");
			
			if(async.isDone) {
			    FlowStateBase fs = FlowStateMachine.GetCurrentFlowState();
			    //doing the test against the race type rather than level name allows us to use the same level for different race types
			    //	e.g. FirstRun for tutorial, or using a new track			-AH
			    if(raceType == "tutorial")
			    {
			    	FlowStateBase.FollowFlowLinkNamed("TutorialExit");
				}

			    else if(raceType == "trainRescue")
			    {
				    FlowStateBase.FollowFlowLinkNamed("TrainExit");
				}
				else
				{
					FlowStateBase.FollowFlowLinkNamed("RaceExit");
				}
			} 
		}
		
	}
}
