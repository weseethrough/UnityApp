﻿using UnityEngine;
using System.Collections;

public class LoadingScreen : MonoBehaviour {
	
	private float rotation = 0f;
	
	AsyncOperation async;
	
	private string levelName;
	protected string raceType;
	
	// Use this for initialization
	void Start () {
		raceType = (string)DataVault.Get("race_type");
		switch(raceType) {
		case "race":
		{
			//if we have a track, load Race Mode, otherwise, load FirstRun. N.B. the menu flow will be different, so it isn't exactly the same FirstRun experience
			Track track = (Track)DataVault.Get("current_track");
			if(track != null)
			{
				levelName = "Race Mode";
			}
			else
			{
				levelName = "FirstRun";	
			}
			break;
		}	
		case "pursuit":
			levelName = "Pursuit Mode";
			break;
			
		case "tutorial":
			levelName = "FirstRun";
			break;
			
		case "trainRescue":
			levelName = "TrainRescue";
			break;
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
		
		transform.rotation = Quaternion.Euler(0, 0, rotation);
		
		if(async != null && async.isDone) {
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			//doing the test against the race type rather than level name allows us to use the same level for different race types
			//	e.g. FirstRun for tutorial, or using a new track			-AH
			if(raceType == "tutorial")
			{
				GConnector gConnect = fs.Outputs.Find(r => r.Name == "TutorialExit");
				if(gConnect != null)
				{
					fs.parentMachine.FollowConnection(gConnect);
				}
				else 
				{
					UnityEngine.Debug.LogWarning("LoadingScreen: error finding tutorial exit");
				}
			}
			else if(raceType == "trainRescue")
			{
				GConnector gConnect = fs.Outputs.Find(r => r.Name == "TrainExit");
				if(gConnect != null)
				{
					fs.parentMachine.FollowConnection(gConnect);	
				}
				else
				{
					UnityEngine.Debug.LogWarning("LoadingScreen: error finding train exit");
				}
			}
			else
			{
				GConnector gConnect = fs.Outputs.Find(r => r.Name == "RaceExit");
				if(gConnect != null)
				{
					fs.parentMachine.FollowConnection(gConnect);
				} 
				else 
				{
					UnityEngine.Debug.LogWarning("LoadingScreen: error finding race exit");	
				}
			}

		}
		
	}
}
