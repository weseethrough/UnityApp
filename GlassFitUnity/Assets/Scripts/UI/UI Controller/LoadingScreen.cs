using UnityEngine;
using System.Collections;

public class LoadingScreen : MonoBehaviour {
	
	private float rotation = 0f;
	
	AsyncOperation async;
	
	private string levelName;
	
	private UISlider slider;
	
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
			levelName = "FirstRun";
			break;
			
		case "trainRescue":
			levelName = "TrainRescue";
			break;
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
			if(async.isDone) {
				FlowState fs = FlowStateMachine.GetCurrentFlowState();
				if(levelName == "FirstRun")
				{
					GConnector gConnect = fs.Outputs.Find(r => r.Name == "TutorialExit");
					if(gConnect != null)
					{
						UnityEngine.Debug.Log("LoadingScreen: connection found, following");
						fs.parentMachine.FollowConnection(gConnect);
					}
					else 
					{
						UnityEngine.Debug.LogWarning("LoadingScreen: error finding tutorial exit");
					}
				}
				else if(levelName == "TrainRescue")
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
	
			} else 
			{
				float progress = async.progress * 100f;
				if(slider != null) {
					slider.Set(progress/100f, false);
				}
				UnityEngine.Debug.Log("LoadingScreen: Loading - " + progress.ToString("f0") + "%");
			}
		}
		
	}
}
