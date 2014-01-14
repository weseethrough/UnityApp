using UnityEngine;
using System.Collections;

public class ChallengeNotification {
	
	private Notification notification;
	private Challenge challenge;
	private User user;
	
	public ChallengeNotification() {}
	public ChallengeNotification(Notification notification, Challenge challenge, User user)
	{
		this.notification = notification;
		this.challenge = challenge;
		this.user = user;
	}
	
	public string GetName() 
	{
		if(user != null) {
			return user.name;
		} else {
			return string.Empty;
		}
	}
	
	public double GetDistance()
	{
		if(challenge != null) 
		{
			if(challenge is DistanceChallenge) 
			{
				DistanceChallenge chall = challenge as DistanceChallenge;
				return chall.distance;
			} 
			else 
			{
				return 0;
			}
		} 
		else 
		{
			return 0;
		}
	}
	
	public int GetTime() {
		if(challenge != null)
		{
			if(challenge is DistanceChallenge) 
			{
				DistanceChallenge chall = challenge as DistanceChallenge;
				return chall.time;
			} 
			else 
			{
				return 0;
			}
		} else 
		{
			return 0;
		}
	}
	
	public void SetRead()
	{
		if(notification != null)
		{
			notification.setRead(true);
		}
	}
	
	public Track GetTrack()
	{
		Track track = challenge.UserTrack(user.id);
		return Platform.Instance.FetchTrack(track.deviceId, track.trackId);
	}
	
}
