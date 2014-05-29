using UnityEngine;
using System.Collections;
using RaceYourself.Models;
using Newtonsoft.Json;

public class MobileLoadingScreen : MonoBehaviour {
	
	private float rotation = 0f;
	
	AsyncOperation async;
	
	private string levelName;

	private UISlider slider;
	
	// Use this for initialization
	void Start () {
		Track track = (Track)DataVault.Get("current_track");

		levelName = "Race Mode";

		slider = GetComponentInChildren<UISlider>();
		
		if(slider != null)	
		{
			slider.Set(0, false);
		}
		
		UnityEngine.Debug.Log("MobileLoadingScreen: Attempting to load level: " + levelName);

		StartCoroutine("LoadLevel");

		//set that we're doing a time-based goal here
		// TODO move this to where the challenge is pressed
		DataVault.Set("goal_type", "time");

	}
	
	IEnumerator LoadLevel() {
		async = Application.LoadLevelAsync(levelName);

		//yield execution until the load is complete
		yield return async;

		//wait a short time so that the scene can start properly before proceeding to the menu
		yield return new WaitForSeconds(0.2f);

		FlowState.FollowFlowLinkNamed("RaceExit");
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

		}
		
	}
	
}
