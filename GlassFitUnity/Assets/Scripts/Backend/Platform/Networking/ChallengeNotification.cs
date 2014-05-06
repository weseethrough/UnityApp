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
