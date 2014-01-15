﻿using UnityEngine;
using System.Collections;

public class LoadingScreen : MonoBehaviour {
	
	private float rotation = 0f;
	
	AsyncOperation async;
	
	private string levelName;
	
	// Use this for initialization
	void Start () {
		switch((string)DataVault.Get("race_type")) {
		case "race":
			levelName = "Race Mode";
			break;
			
		case "pursuit":
			levelName = "Pursuit Mode";
			break;
			
		case "tutorial":
			levelName = "TrainRescue";
			break;
		}
		
		//just force train rescue to load anyway
		levelName = "TrainRescue";
		
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
			if(levelName == "FirstRun")
			{
				GConnector gConnect = fs.Outputs.Find(r => r.Name == "TutorialExit");
				if(gConnect != null)
				{
					fs.parentMachine.FollowConnection(gConnect);
				}
				else 
				{
					UnityEngine.Debug.Log("LoadingScreen: error finding tutorial exit");
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
					UnityEngine.Debug.Log("LoadingScreen: error finding race exit");	
				}
			}

		}
		
	}
}
