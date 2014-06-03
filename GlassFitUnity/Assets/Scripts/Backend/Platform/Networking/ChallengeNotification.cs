using UnityEngine;
using System.Collections;

using RaceYourself.Models;

public class ChallengeNotification {
	
	private Notification notification;
	private Challenge challenge;
	private User user;
	private Track track;
	
	public ChallengeNotification() {}
	public ChallengeNotification(Notification notification, Challenge challenge, User user, Track track)
	{
		this.notification = notification;
		this.challenge = challenge;
		this.user = user;
		this.track = track;
	}
	public ChallengeNotification(Notification notification, Challenge challenge, User user) 
	{
		this.notification = notification;
		this.challenge = challenge;
		this.user = user;
		this.track = null;
	}
	
	public string GetName() 
	{
		if(user != null) {
			return user.name;
		} else {
			return string.Empty;
		}
	}
	
	public int GetID() {
		if(notification != null) {
			return notification.id;
		} else {
			return -1;
		}
	}

	public User GetUser() {
		if(user != null) {
			return user;
		} 
		else 
		{
			UnityEngine.Debug.LogError("ChallengeNotification: user is null");
			return null;
		}
	}

	public Notification GetNotification()
	{
		if(notification != null) 
		{
			return notification;
		}
		else
		{
			UnityEngine.Debug.LogError("ChallengeNotification: notification is null");
			return null;
		}
	}

	public Challenge GetChallenge() {
		if(challenge != null)
		{
			return challenge;
		}
		else
		{
			UnityEngine.Debug.LogError("ChallengeNotification: challenge is null");
			return null;
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

	public int GetDuration() {
		if(challenge != null) 
		{
			if(challenge is DurationChallenge) 
			{
				DurationChallenge chall = challenge as DurationChallenge;
				return chall.duration;
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
		} 
		else 
		{
			return 0;
		}
	}
	
	public void SetRead()
	{
		if(notification != null)
		{
			Platform.Instance.ReadNotification(notification.id);
		}
	}
	
	public Track GetTrack()
	{
		if(track != null) {
			return track;
		} 
		else 
		{
			return null;
		}
	}	
}
