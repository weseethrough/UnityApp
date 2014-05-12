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
				FlowState fs = FlowStateMachine.GetCurrentFlowState();
				FlowState.FollowFlowLinkNamed("RaceExit");
			} 
		}
		
	}
}
