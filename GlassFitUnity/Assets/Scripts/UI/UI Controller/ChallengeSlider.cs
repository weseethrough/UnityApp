using UnityEngine;
using System.Collections;

using RaceYourself.Models;

public class ChallengeSlider : MonoBehaviour {

	UITexture friendTexture;
	UITexture userTexture;

	// Use this for initialization
	void Start () {
//		UISlider slider = GetComponentInChildren<UISlider>();
//		if(slider != null) {
//			EventDelegate.Add(slider.onChange, SetValue);
//		}
		Panel currentPanel = FlowStateMachine.GetCurrentFlowState() as Panel;
		Friend chosenFriend = (Friend)DataVault.Get("chosen_friend");
		string temp = "test"; 
		if(chosenFriend != null) {
			DataVault.Set("friend_name", chosenFriend.name);
			GameObject friendPicture = GameObjectUtils.SearchTreeByName(currentPanel.physicalWidgetRoot, "RivalPicture");
			if(friendPicture != null) {
				friendTexture = friendPicture.GetComponentInChildren<UITexture>();
			}

			if(friendTexture != null) {
				Platform.Instance.RemoteTextureManager.LoadImage(chosenFriend.image, temp, (tex, button) => {
					friendTexture.mainTexture = tex;
				});
			}
		}

		GameObject userPicture = GameObjectUtils.SearchTreeByName(currentPanel.physicalWidgetRoot, "PlayerPicture");
		if(userPicture != null) {
			userTexture = userPicture.GetComponentInChildren<UITexture>();
		}
		if(userTexture != null) {
			Platform.Instance.RemoteTextureManager.LoadImage(Platform.Instance.User().image, temp, (tex, button) => {
				userTexture.mainTexture = tex;
			});
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetValue() {
		float convertedSliderValue = (UISlider.current.value * 5) + 1;
		int runTime = (int)(convertedSliderValue * 5);

		DataVault.Set("run_time", runTime);
//		Debug.LogError(UISlider.current.value);
	}
}
