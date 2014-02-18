using UnityEngine;
using System.Collections;
using SimpleJSON;

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
			break;
			
		case "challenge":
			levelName = "Challenge Mode";
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
			
		default:
			UnityEngine.Debug.Log("LoadingScreen: ERROR: Unknown race_type: " + raceType);
			break;
		}
		
		if (Platform.Instance.IsDisplayRemote()) {
            JSONObject json = new JSONObject();
			json.AddField("action", "LoadLevelAsync");
			
			JSONObject data = new JSONObject();
			if (track != null) data.AddField("current_track", track.AsJson);
			else data.AddField("current_track", (JSONObject)null);
			data.AddField("race_type", raceType);
			if (DataVault.Get("type") != null) data.AddField("type", DataVault.Get("type") as string);
			if (DataVault.Get("finish") != null) data.AddField("finish", (int)DataVault.Get("finish"));
			if (DataVault.Get("lower_finish") != null) data.AddField("lower_finish", (int)DataVault.Get("lower_finish"));
			if (DataVault.Get("challenger") != null) data.AddField("challenger", DataVault.Get("challenger") as string);
			if (DataVault.Get("current_challenge_notification") != null) data.AddField("current_challenge_notification", (DataVault.Get("current_challenge_notification") as ChallengeNotification).AsJson);
			
			json.AddField("data", data);
			Platform.Instance.BluetoothBroadcast(json);
			MessageWidget.AddMessage("Bluetooth", "Started game on Glass", "settings");
			// Return to menu
		    FlowState fs = FlowStateMachine.GetCurrentFlowState();
		    GConnector gConnect = fs.Outputs.Find(r => r.Name == "MenuExit");
			if (gConnect != null) {
				fs.parentMachine.FollowConnection(gConnect);
				return;
			}
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
            else 
			{
				
			}
		}
		
	}
}
