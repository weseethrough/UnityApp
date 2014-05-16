using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

using RaceYourself.Models;

[Serializable]
public class MobileChallengeInfoPanel : MobilePanel {

	/// <summary>
	/// default constructor
	/// </summary>
	/// <returns></returns>
	public MobileChallengeInfoPanel() : base() {}
	
	/// <summary>
	/// deserialziation constructor
	/// </summary>
	/// <param name="info">seirilization info conataining class data</param>
	/// <param name="ctxt">serialization context </param>
	/// <returns></returns>
	public MobileChallengeInfoPanel(SerializationInfo info, StreamingContext ctxt)
		: base(info, ctxt)
	{    
		
	}

	/// <summary>
	/// Gets display name of the node, helps with node identification in editor
	/// </summary>
	/// <returns>name of the node</returns>
	public override string GetDisplayName()
	{
		base.GetDisplayName();
		
		GParameter gName = Parameters.Find(r => r.Key == "Name");
		if (gName != null)
		{
			return "MobileChallengeInfoPanel: " + gName.Value;
		}
		return "MobileChallengeInfoPanel: UnInitialzied";
	}

	public override void EnterStart ()
	{
		base.EnterStart ();

		Notification challengeNotification = (Notification)DataVault.Get("challenge_notification");

		if(challengeNotification != null) {
			User rival = Platform.Instance.GetUser(challengeNotification.message.from);
			if(rival.id == Platform.Instance.User().id) {
				rival = Platform.Instance.GetUser(challengeNotification.message.to);
			}

			GameObject rivalPicture = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalPicture");
			string empty = "test";
			if(rivalPicture != null) {
				UIBasiclabel rivalName = rivalPicture.GetComponent<UIBasiclabel>();
				if(rivalName != null) {
					rivalName.SetLabel(rival.forename);
				}

				UITexture rivalPictureTex = rivalPicture.GetComponentInChildren<UITexture>();
				UnityEngine.Debug.LogError(Platform.Instance.User().name);
				if(rivalPictureTex != null) {
					Platform.Instance.RemoteTextureManager.LoadImage(rival.image, empty, (tex, button) => {
						rivalPictureTex.mainTexture = tex;
					});
				}
			}

			GameObject userPicture = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerPicture");
			if(userPicture != null) {
				UITexture userPictureTex = userPicture.GetComponentInChildren<UITexture>();
				if(userPictureTex != null) {
					Platform.Instance.RemoteTextureManager.LoadImage(Platform.Instance.User().image, empty, (tex, button) => {
						userPictureTex.mainTexture = tex;
					});
				}
			}

			DataVault.Set("result", "");

			Challenge challenge = Platform.Instance.FetchChallenge(challengeNotification.message.challenge_id);

			int duration = (challenge as DurationChallenge).duration;
			if(duration > 120) {
				duration /= 60;
			}
			DataVault.Set("duration", duration.ToString());

			GameObject playerReward = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerReward");
			if(playerReward != null) {
				playerReward.SetActive(false);
			}

			GameObject rivalReward = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalReward");
			if(rivalReward != null) {
				rivalReward.SetActive(false);
			}
		}
	}
}
