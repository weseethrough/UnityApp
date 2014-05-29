using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

using RaceYourself.Models;

[Serializable]
public class MobileChallengeInfoPanel : MobilePanel {

	private Track playerTrack;
	private Track rivalTrack;

	double playerDistance = 0;
	double rivalDistance = 0;

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

		string empty = "test";

		if(challengeNotification != null) {
			User player = Platform.Instance.User();
			//User rival = null; 

			int rivalID = challengeNotification.message.from;
			if(rivalID == player.id)
			{
				rivalID = challengeNotification.message.to;
			}

			Platform.Instance.GetUser(rivalID, (rival) => {

				DataVault.Set("chosen_user", rival);

				GameObject rivalPicture = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalPicture");
				if(rivalPicture != null) {
					UIBasiclabel rivalName = rivalPicture.GetComponent<UIBasiclabel>();
					if(rivalName != null) {
						rivalName.SetLabel(rival.forename);
					}

					UITexture rivalPictureTex = rivalPicture.GetComponentInChildren<UITexture>();
	//				UnityEngine.Debug.LogError(Platform.Instance.User().name);
					if(rivalPictureTex != null) {
						Platform.Instance.RemoteTextureManager.LoadImage(rival.image, "test", (tex, button) => {
							rivalPictureTex.mainTexture = tex;
						});
					}
				}
			});

			GameObject userPicture = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerPicture");
			if(userPicture != null) {
				UITexture userPictureTex = userPicture.GetComponentInChildren<UITexture>();
				if(userPictureTex != null) {
					Platform.Instance.RemoteTextureManager.LoadImage(player.image, empty, (tex, button) => {
						userPictureTex.mainTexture = tex;
					});
				}
			}

			DataVault.Set("result", "");

			Platform.Instance.FetchChallenge(challengeNotification.message.challenge_id, (challenge) => {
				if(challenge != null) {
					int duration = (challenge as DurationChallenge).duration;
					if(duration > 120) {
						duration /= 60;
					}
					DataVault.Set("duration", duration.ToString());
					DataVault.Set("finish_time_seconds", duration*60);
					
					GameObject playerReward = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerReward");
					if(playerReward != null) {
						playerReward.SetActive(false);
					}
					
					GameObject rivalReward = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalReward");
					if(rivalReward != null) {
						rivalReward.SetActive(false);
					}
					
					List<Challenge.Attempt> attempts = challenge.attempts;
					
					playerDistance = 0;
					rivalDistance = 0;
					
					bool playerComplete = false;
					bool rivalComplete = false;
					
					if(attempts != null && attempts.Count > 0) {
						foreach(Challenge.Attempt attempt in attempts) {
							if(attempt.user_id == player.id) {
								GameObject raceNowBtn = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RaceNowBtn");
								if(raceNowBtn != null) {
									raceNowBtn.SetActive(false);
								}
								playerComplete = true;
								playerTrack = Platform.Instance.FetchTrack(attempt.device_id, attempt.track_id);
								if(playerTrack != null) {
									playerDistance = playerTrack.distance;
									GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerDistanceText", (playerDistance / 1000).ToString("f2"));
								}
							} else if(attempt.user_id == rivalID) {
								rivalComplete = true;
								rivalTrack = Platform.Instance.FetchTrack(attempt.device_id, attempt.track_id);
								if(rivalTrack != null) {
									rivalDistance = rivalTrack.distance;
									DataVault.Set("current_track", rivalTrack);
									GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "RivalDistanceText", (rivalDistance / 1000).ToString("f2"));
								}
							}
						}
					}
					
					if(playerComplete && rivalComplete) {
						if(playerDistance > rivalDistance) {
							if(playerReward != null) {
								playerReward.SetActive(true);
							}
						} else {
							if(rivalReward != null) {
								rivalReward.SetActive(true);
							}
						}
					}
				}
			});



		}
	}
}
