using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

using RaceYourself.Models;

[Serializable]
public class MobileChallengeInfoPanel : MobilePanel {

//	private Track playerTrack;
	private Track rivalTrack;

	double playerDistance = 0;
	double rivalDistance = 0;

	Notification challengeNotification;

	GameObject rewardContainer;
	GameObject shareContainer;
	GameObject raceButtonContainer;
	GameObject expireContainer;

	double yourPaceValue;
	double yourSpeedValue;
	double yourDownValue;
	double yourUpValue;

	double rivalPaceValue;
	double rivalSpeedValue;
	double rivalDownValue;
	double rivalUpValue;

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

	private IEnumerator CheckChallengeAttempts() 
	{
		User player = Platform.Instance.User();

		DataVault.Set("title", "");
		DataVault.Set("description", "");

		int rivalId = challengeNotification.message.from;
		if(rivalId == player.id) 
		{
			rivalId = challengeNotification.message.to;
		}

		User rival = null;

		yield return Platform.Instance.partner.StartCoroutine(Platform.Instance.GetUserCoroutine(rivalId, (u) => {
			rival = u;
		}));

		DataVault.Set("opponent_user", rival);
			
		DataVault.Set("rival", rival.DisplayName);

		if(challengeNotification.message.from == player.id) 
		{
			DataVault.Set("title", "You challenged");
			DataVault.Set("description", rival.DisplayName);
		} else {
			DataVault.Set("title", rival.DisplayName);
			DataVault.Set("description", "Has sent you a challenge!");
			DataVault.Set("header_text", "Incoming Challenge");
		}

		GameObject rivalPicture = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerPicture");
		if(rivalPicture != null) 
		{
			UITexture rivalPictureTex = rivalPicture.GetComponentInChildren<UITexture>();
			//					UnityEngine.Debug.LogError(Platform.Instance.User().name);
			if(rivalPictureTex != null) 
			{
				Platform.Instance.RemoteTextureManager.LoadImage(rival.image, null, (tex, arg) => 				                                                 {
					rivalPictureTex.mainTexture = tex;
				});
			}
		}
			
		Challenge challenge = null;

		yield return Platform.Instance.partner.StartCoroutine(Platform.Instance.FetchChallengeCoroutine(challengeNotification.message.challenge_id, (c) => {
			challenge = c;
		}));

		if(challenge != null) 
		{
			UnityEngine.Debug.Log("MobileChallengeInfoPanel: challenge id is " + challenge.id);
			int duration = (challenge as DurationChallenge).duration / 60;
			DataVault.Set("duration", duration.ToString());
			DataVault.Set("challenge_title", "Who can run the furthest in [39BC3C]" + duration.ToString() + "[-] min?");
			DataVault.Set("finish_time_seconds", duration * 60);

			DataVault.Set("expires", challenge.stop_time.Value.ToString("O"));

			List<ChallengeAttempt> attempts = challenge.attempts;
			UnityEngine.Debug.Log("MobileChallengeInfoPanel: number of attempts is " + attempts.Count);

			playerDistance = 0;
			rivalDistance = 0;

			bool playerComplete = false;
			bool rivalComplete = false;
				
			GameObject yourCircle = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "LeftCircle");
			GameObject rivalsCircle = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RightCircle");

			if(attempts != null && attempts.Count > 0) 
			{
				UnityEngine.Debug.LogError(attempts.Count);
				foreach(ChallengeAttempt attempt in attempts) 
				{
					if(attempt.user_id == player.id && !playerComplete) 
					{
						raceButtonContainer = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RaceButtonContainer");
						if(raceButtonContainer != null)
						{
							raceButtonContainer.SetActive(false);
						}
						else
						{
							UnityEngine.Debug.LogError("MobileChallengeInfoPanel: race button container is null");
						}
							
						playerComplete = true;
						Track playerTrack = null;
						yield return Platform.Instance.partner.StartCoroutine( Platform.Instance.FetchTrackCoroutine(attempt.device_id, attempt.track_id, (track) => {
							playerTrack = track;
						}));
						if(playerTrack != null) 
						{
							playerDistance = playerTrack.distance;
                            DataVault.Set("your_distance", Math.Round(playerTrack.distance/1000, 2).ToString());
							DataVault.Set("your_distance_sub", "KM");
							DataVault.Set("rival_empty_run", "");
							DataVault.Set("your_time", playerTrack.date.ToString("ran h:mm tt dd.MM.yy").ToLower());

							float? init_alt = null;
							float min_alt = float.MaxValue;
							float max_alt = float.MinValue;
							float max_speed = 0;
							foreach (var position in playerTrack.positions) {
								if (position.alt.HasValue && !init_alt.HasValue) init_alt = position.alt;
								if (position.alt.HasValue && max_alt < position.alt) max_alt = position.alt.Value;
								if (position.alt.HasValue && min_alt > position.alt) min_alt = position.alt.Value;
								if (position.speed > max_speed) max_speed = position.speed;
                            }
							yourPaceValue = Math.Round((playerTrack.distance*60*60/1000) / playerTrack.time, 1);
							DataVault.Set("your_pace", yourPaceValue.ToString());
							yourSpeedValue = Math.Round((max_speed*60*60)/1000, 1);
							DataVault.Set("your_speed", yourSpeedValue.ToString());
							if (init_alt.HasValue) {
								yourUpValue = Math.Round(max_alt - init_alt.Value, 1);
								DataVault.Set("your_up", yourUpValue.ToString());
								yourDownValue = Math.Round(init_alt.Value - min_alt, 1);
								DataVault.Set("your_down", yourDownValue.ToString());
                            }
                            DataVault.Set("your_weather", "?");
							// TODO: Remove duplicated code
						}
                        
                    } else if(attempt.user_id == rival.id) 
					{
						rivalComplete = true;
						Track rivalTrack = null;
						yield return Platform.Instance.partner.StartCoroutine( Platform.Instance.FetchTrackCoroutine(attempt.device_id, attempt.track_id, (track) => {
							rivalTrack = track;
						}));

						if(rivalTrack != null) 
						{
							rivalDistance = rivalTrack.distance;
							DataVault.Set("rivals_distance", Math.Round(rivalTrack.distance/1000, 2).ToString());
							DataVault.Set("rivals_distance_sub", "KM");						
							DataVault.Set("rivals_time", rivalTrack.date.ToString("ran h:mm tt dd.MM.yy").ToLower());

							float? init_alt = null;
							float min_alt = float.MaxValue;
							float max_alt = float.MinValue;
							float max_speed = 0;
							foreach (var position in rivalTrack.positions) {
								if (position.alt.HasValue && !init_alt.HasValue) init_alt = position.alt;
								if (position.alt.HasValue && max_alt < position.alt) max_alt = position.alt.Value;
								if (position.alt.HasValue && min_alt > position.alt) min_alt = position.alt.Value;
								if (position.speed > max_speed) max_speed = position.speed;
							}
							rivalPaceValue = Math.Round((rivalTrack.distance*60*60/1000) / rivalTrack.time, 1);
							DataVault.Set("rivals_pace", rivalPaceValue.ToString());
							rivalSpeedValue = Math.Round((max_speed*60*60)/1000, 1);
							DataVault.Set("rivals_speed", rivalSpeedValue.ToString());
							if (init_alt.HasValue) {
								rivalUpValue = Math.Round(max_alt - init_alt.Value, 1);
								DataVault.Set("rivals_up", rivalUpValue.ToString());
								rivalDownValue =  Math.Round(init_alt.Value - min_alt, 1);
                                DataVault.Set("rivals_down", rivalDownValue.ToString());
                            }
                            DataVault.Set("rivals_weather", "?");
                            // TODO: Remove duplicated code
                        }
                    }
                }
            }

			GameObject playerPace = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "YoursPace");
			GameObject playerSpeed = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "YoursSpeed");
			GameObject playerDown = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "YoursTotalDown");
			GameObject playerUp = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "YoursTotalUp");

			GameObject rivalPace = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalPace");
			GameObject rivalSpeed = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalSpeed");
			GameObject rivalDown = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalTotalDown");
			GameObject rivalUp = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalTotalUp");


			if (!playerComplete) {
				DataVault.Set("your_empty_run", "NO RUN RECORDED");
				yourCircle.GetComponent<UITexture>().color = Palette.grey;
            }
			else
			{
				playerPace.GetComponent<UILabel>().color = Palette.green;
				playerSpeed.GetComponent<UILabel>().color = Palette.green;
				playerDown.GetComponent<UILabel>().color = Palette.green;
				playerUp.GetComponent<UILabel>().color = Palette.green;
				yourCircle.GetComponent<UITexture>().color = Palette.green;
			}

			if (!rivalComplete) {
				DataVault.Set("rival_empty_run", "NO RUN RECORDED");
				rivalsCircle.GetComponent<UITexture>().color = Palette.grey;
			}
			else
			{
				rivalPace.GetComponent<UILabel>().color = Palette.green;
				rivalSpeed.GetComponent<UILabel>().color = Palette.green;
				rivalDown.GetComponent<UILabel>().color = Palette.green;
				rivalUp.GetComponent<UILabel>().color = Palette.green;
				rivalsCircle.GetComponent<UITexture>().color = Palette.green;
			}
            
//			GameObject shareContainer = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "ShareContainer");
            if(playerComplete && rivalComplete) 
			{
				if(yourPaceValue > rivalPaceValue)
				{
					playerPace.GetComponent<UILabel>().color = Palette.green;
					rivalPace.GetComponent<UILabel>().color = Palette.red;
				}
				else
				{
					playerPace.GetComponent<UILabel>().color = Palette.red;
					rivalPace.GetComponent<UILabel>().color = Palette.green;
				}

				if(yourSpeedValue > rivalSpeedValue)
				{
					playerSpeed.GetComponent<UILabel>().color = Palette.green;
					rivalSpeed.GetComponent<UILabel>().color = Palette.red;
				}
				else
				{
					playerSpeed.GetComponent<UILabel>().color = Palette.red;
					rivalSpeed.GetComponent<UILabel>().color = Palette.green;
				}

				if(yourUpValue > rivalUpValue)
				{
					playerUp.GetComponent<UILabel>().color = Palette.green;
					rivalUp.GetComponent<UILabel>().color = Palette.red;
				}
				else
				{
					playerUp.GetComponent<UILabel>().color = Palette.red;
					rivalUp.GetComponent<UILabel>().color = Palette.green;
				}

				if(yourDownValue > rivalDownValue)
				{
					playerDown.GetComponent<UILabel>().color = Palette.green;
					rivalDown.GetComponent<UILabel>().color = Palette.red;
				}
				else
				{
					playerDown.GetComponent<UILabel>().color = Palette.red;
					rivalDown.GetComponent<UILabel>().color = Palette.green;
				}

				GameObject challengeTitle = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "ChallengeTitle");
				expireContainer = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "ExpireContainer");
				expireContainer.SetActive(false);
				DataVault.Set("description", "Challenge Complete");
				DataVault.Set("header_text", "Challenge Complete");
				if(playerDistance > rivalDistance) 
				{
					DataVault.Set("share_title", "time to bask in the glory of success!");
					DataVault.Set("social_description", "I beat " + rival.forename + " by running " + (playerDistance / 1000).ToString("f2") + "km in " + duration.ToString() + "mins!");
					DataVault.Set("social_result", "I beat " + rival.forename + "!");
					yourCircle.GetComponent<UITexture>().color = Palette.green;
					rivalsCircle.GetComponent<UITexture>().color = Palette.red;
					rewardContainer.SetActive(true);
					challengeTitle.GetComponent<UILabel>().color = Palette.green;
					DataVault.Set("challenge_title", "You Won - " + (playerDistance / 1000).ToString("f2") + "km in " + duration.ToString() + " mins");
				} else 
				{
					DataVault.Set("share_title", "congratulate your fellow competitor!");
					DataVault.Set("social_description", rival.forename + " beat me by running " + (rivalDistance / 1000).ToString("f2") + "km in " + duration.ToString() + "mins!");
					DataVault.Set("social_result", rival.forename + " beat me");
					yourCircle.GetComponent<UITexture>().color = Palette.red;
					rivalsCircle.GetComponent<UITexture>().color = Palette.green;
					DataVault.Set("challenge_title", "You Lost - " + (rivalDistance / 1000).ToString("f2") + "km in " + duration.ToString() + " mins");
				}
				GameObject card = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "DropCard");
				var cardPosition = card.transform.localPosition;
				cardPosition.y = -65; // TODO: Remove magic variable
				card.transform.localPosition = cardPosition;
				shareContainer.SetActive(true);
            } 
		}
	}

	public override void EnterStart ()
	{
		base.EnterStart ();

		challengeNotification = (Notification)DataVault.Get("challenge_notification");

		DataVault.Set("your_pace", "- -");
		DataVault.Set("your_speed", "- -");
		DataVault.Set("your_up", "- -");
		DataVault.Set("your_down", "- -");
		DataVault.Set("your_weather", "");
		DataVault.Set("your_distance", "");
		DataVault.Set("your_distance_sub", "");
		DataVault.Set("your_empty_run", "");
		DataVault.Set("rivals_pace", "- -");
		DataVault.Set("rivals_speed", "- -");
		DataVault.Set("rivals_up", "- -");
		DataVault.Set("rivals_down", "- -");
		DataVault.Set("rivals_weather", "- -");
		DataVault.Set("rivals_distance", "");
		DataVault.Set("rivals_distance_sub", "");
		DataVault.Set("rival_empty_run", "");
        
		DataVault.Set("header_text", "Challenge");
		DataVault.Set("challenge_title", "Who can run the furthest?");

		rewardContainer = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RewardContainer");
        if(rewardContainer != null)
		{
			rewardContainer.SetActive(false);
		}
		else
		{
			UnityEngine.Debug.LogError("MobileChallengeInfoPanel: couldn't find reward container!");
		}

		shareContainer = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "ShareContainer");
		if(shareContainer != null)
		{
			shareContainer.SetActive(false);
		}
		else
		{
			UnityEngine.Debug.LogError("MobileChallengeInfoPanel: couldn't find share container!");
		}

		DataVault.Set("your_time", "");
		DataVault.Set("rival", "?");
		DataVault.Set("rivals_time", "");

//		parentMachine.ForbidBack();

        if(challengeNotification != null) 
		{
			Platform.Instance.partner.StartCoroutine(CheckChallengeAttempts());
			Platform.Instance.ReadNotification(challengeNotification.id);
			challengeNotification.read = true;
		}
	}
}
