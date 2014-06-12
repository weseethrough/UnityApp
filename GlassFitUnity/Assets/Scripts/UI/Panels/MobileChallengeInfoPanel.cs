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
			int duration = (challenge as DurationChallenge).duration;
			if(duration > 120) 
			{
				duration /= 60;
			}
			DataVault.Set("duration", duration.ToString());
			DataVault.Set("finish_time_seconds", duration * 60);

			DataVault.Set("expires", challenge.stop_time.Value.ToString("O"));

			List<Challenge.Attempt> attempts = challenge.attempts;
			UnityEngine.Debug.Log("MobileChallengeInfoPanel: number of attempts is " + attempts.Count);

			playerDistance = 0;
			rivalDistance = 0;

			bool playerComplete = false;
			bool rivalComplete = false;
				
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
						Track playerTrack = null;
						yield return Platform.Instance.partner.StartCoroutine( Platform.Instance.FetchTrackCoroutine(attempt.device_id, attempt.track_id, (track) => {
							playerTrack = track;
						}));
						if(playerTrack != null) 
						{
							playerDistance = playerTrack.distance;
                            DataVault.Set("your_distance", Math.Round(playerTrack.distance/1000, 2));
							DataVault.Set("your_distance_sub", "KM");
							DataVault.Set("your_time", "ran " + playerTrack.date.ToString("ran h:mm tt dd.MM.yy").ToLower());
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
							DataVault.Set("rivals_distance", Math.Round(rivalTrack.distance/1000, 2) + " KM");
							DataVault.Set("rivals_distance_sub", "KM");						
							DataVault.Set("rivals_time", "ran " + rivalTrack.date.ToString("ran h:mm tt dd.MM.yy").ToLower());
                        }
                    }
                }
			}

			if (!playerComplete) {
				DataVault.Set("your_distance_sub", "NO RUN RECORDED");
            }
			if (!rivalComplete) {
				DataVault.Set("rivals_distance_sub", "NO RUN RECORDED");
            }
            
            if(playerComplete && rivalComplete) 
			{
				if(playerDistance > rivalDistance) 
				{
					// TODO
				} else 
				{
					// TODO
				}
			}
		}
	}

	public override void EnterStart ()
	{
		base.EnterStart ();

		challengeNotification = (Notification)DataVault.Get("challenge_notification");

		DataVault.Set("your_pace", "?");
		DataVault.Set("your_speed", "?");
		DataVault.Set("your_up", "?");
		DataVault.Set("your_down", "?");
		DataVault.Set("your_weather", "?");
		DataVault.Set("your_distance", "");
		DataVault.Set("your_distance_sub", "");
        DataVault.Set("rivals_pace", "?");
		DataVault.Set("rivals_speed", "?");
		DataVault.Set("rivals_up", "?");
		DataVault.Set("rivals_down", "?");
		DataVault.Set("rivals_weather", "?");
		DataVault.Set("rivals_distance", "");
		DataVault.Set("rivals_distance_sub", "");
        
        
        DataVault.Set("your_time", "");
		DataVault.Set("rival", "?");
		DataVault.Set("rivals_time", "");

        if(challengeNotification != null) 
		{
			Platform.Instance.partner.StartCoroutine(CheckChallengeAttempts());
			Platform.Instance.ReadNotification(challengeNotification.id);
			challengeNotification.read = true;
		}
	}
}
