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

		if(challengeNotification != null) 
		{
			User player = Platform.Instance.User();
			int rivalId = challengeNotification.message.from;
			if(rivalId == player.id) 
			{
				rivalId = challengeNotification.message.to;
			}

			Platform.Instance.GetUser(rivalId, (rival) => {

				DataVault.Set("chosen_user", rival);

				GameObject rivalPicture = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalPicture");
				string empty = "test";
				if(rivalPicture != null) 
				{
					UIBasiclabel rivalName = rivalPicture.GetComponent<UIBasiclabel>();
					if(rivalName != null) 
					{
						GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "RivalName", rival.forename);
						rivalName.SetLabel(rival.forename);
					}

					UITexture rivalPictureTex = rivalPicture.GetComponentInChildren<UITexture>();
//					UnityEngine.Debug.LogError(Platform.Instance.User().name);
					if(rivalPictureTex != null) 
					{
						Platform.Instance.RemoteTextureManager.LoadImage(rival.image, empty, (tex, button) => 
						{
							rivalPictureTex.mainTexture = tex;
						});
					}
				}

				GameObject userPicture = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerPicture");
				if(userPicture != null) 
				{
					UITexture userPictureTex = userPicture.GetComponentInChildren<UITexture>();
					if(userPictureTex != null) 
					{
						Platform.Instance.RemoteTextureManager.LoadImage(player.image, empty, (tex, button) => 
				    	{
							userPictureTex.mainTexture = tex;
						});
					}
				}

				GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "ChallengeResultText", "How far can you run?");

				Platform.Instance.FetchChallenge(challengeNotification.message.challenge_id, (challenge) => {
					if(challenge != null) 
					{
						int duration = (challenge as DurationChallenge).duration;
						if(duration > 120) 
						{
							duration /= 60;
						}
						DataVault.Set("duration", duration.ToString());
						GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DurationText", duration.ToString());
						DataVault.Set("finish_time_seconds", duration * 60);
					
						List<Challenge.Attempt> attempts = challenge.attempts;
						UnityEngine.Debug.Log("MobileChallengeInfoPanel: number of attempts is " + attempts.Count);
						
						playerDistance = 0;
						rivalDistance = 0;
						
						bool playerComplete = false;
						bool rivalComplete = false;

						GameObject playerDistanceObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerDistance");
						UITexture playerCircle = playerDistanceObj.GetComponentInChildren<UITexture>();
						GameObject rivalDistanceObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalDistance");
						UITexture rivalCircle = rivalDistanceObj.GetComponentInChildren<UITexture>();
						if(attempts != null && attempts.Count > 0) 
						{
							foreach(Challenge.Attempt attempt in attempts) 
							{
								if(attempt.user_id == player.id && !playerComplete) 
								{
									GameObject raceNowBtn = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RaceNowBtn");
									if(raceNowBtn != null) 
									{
										raceNowBtn.SetActive(false);
									}

									GameObject raceLaterBtn = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "LaterBtn");
									if(raceLaterBtn != null) 
									{
										raceLaterBtn.SetActive(false);
									}

									playerComplete = true;
									playerTrack = Platform.Instance.FetchTrack(attempt.device_id, attempt.track_id);
									if(playerTrack != null) 
									{
										playerDistance = playerTrack.distance;
										GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerDistanceText", (playerDistance / 1000).ToString("f2"));
										GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerDistanceUnitsText", "KM");
										GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerQuestionText", "");
										if(playerCircle != null) {
											playerCircle.color = new Color(57/255f, 188/255f, 60/255f);
										}
									}
								} else if(attempt.user_id == rival.id) 
								{
									rivalComplete = true;
									rivalTrack = Platform.Instance.FetchTrack(attempt.device_id, attempt.track_id);
									if(rivalTrack != null) 
									{
										rivalDistance = rivalTrack.distance;
										DataVault.Set("current_track", rivalTrack);
										GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "RivalDistanceText", (rivalDistance / 1000).ToString("f2"));
										GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "RivalDistanceUnitsText", "KM");
										GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "RivalQuestionText", "");
										if(rivalCircle != null) {
											rivalCircle.color = new Color(57/255f, 188/255f, 60/255f);
										}
									}
								}
							}
						}
						
						if(playerComplete && rivalComplete) 
						{
							GameObject reward = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "Reward");
							if(reward != null)
							{
								UIAnchor rewardAnchor = reward.GetComponent<UIAnchor>();
								if(rewardAnchor != null) 
								{
									if(playerDistance > rivalDistance) 
									{
										rewardAnchor.relativeOffset = new Vector2(-0.15f, 0.35f);
										GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "ChallengeResultText", "You Won!");
										if(playerCircle != null) 
										{
											playerCircle.color = new Color(255/255f, 204/255f, 0);
										} 
										if(rivalCircle != null) 
										{
											rivalCircle.color = new Color(255/255f, 69/255f, 28/255f);
										}
									} else 
									{
										rewardAnchor.relativeOffset = new Vector2(0.15f, 0.35f);
										GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "ChallengeResultText", "You Lost!");
										if(playerCircle != null) 
										{
											playerCircle.color = new Color(255/255f, 69/255f, 28/255f);
										} 
										if(rivalCircle != null) 
										{
											rivalCircle.color = new Color(255/255f, 204/255f, 0);
										}
									}
								}
							}
						}
					}
				});
			});
		}
	}
}
