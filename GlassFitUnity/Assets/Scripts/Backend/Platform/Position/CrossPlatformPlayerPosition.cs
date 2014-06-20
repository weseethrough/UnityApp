using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using RaceYourself.Models;
using PositionTracker;

public class CrossPlatformPlayerPosition : PlayerPosition
{
	private Position _position = null;
	public override Position Position { get { return _position; } }

	private Position _predictedPosition = null;
	public override Position PredictedPosition { get { return _predictedPosition; } }

	private long _time = 0;
	public override long Time { get 
		{
			//Should return _time. However, that's not currently working, so temp workaround to try to retrieve from the datavault. Will only work on mobile atm.
			try {
				float time_seconds = (float)DataVault.Get("elapsed_time");
				long time_milliseconds = (long)(time_seconds * 1000);
				return time_milliseconds;
			} catch (Exception e) {
				return 0;
			}

		} }

	private double _distance = 0.0;
	public override double Distance { get { return _distance; } }

	private float _pace = 0;
	public override float Pace { get { return _pace; } }

	protected float _bearing = -999.0f;
	public override float Bearing { get { return _bearing; } }

	private PositionTracker.PositionTracker positionTracker;
	private IPositionProvider positionProvider;
	private CrossPlatformSensorProvider sensorProvider;


	public CrossPlatformPlayerPosition() {
    
	//need to use a bespoke implementation for iOS since Unity's location input doesn't provide a heading.
	//On Android, until there's an Android position provider, we'll just use the original AndroidPlayerPosition wholesale
		GameObject platform = GameObject.Find("Platform");
#if UNITY_EDITOR
        // in the editor, we fake everything
        log.info ("Adding EditorPositionProvider to Platform game object");
        positionProvider = platform.AddComponent<EditorPositionProvider>();
#elif UNITY_IPHONE
        log.info ("Adding IosPositionProvider to Platform game object");
		positionProvider = platform.AddComponent<IosPositionProvider>();
#elif UNITY_ANDROID
        // should never run, we'll be using AndroidPlayerPosition instead of this class. TODO: fix this!
        log.warning ("THIS SHOULD NEVER HAPPEN: Adding CrossPlatformPositionProvider to Platform game object on ANDROID");
		positionProvider = platform.AddComponent<CrossPlatformPositionProvider>();
#endif
		sensorProvider = platform.AddComponent<CrossPlatformSensorProvider>();

		//Note - unity complains that PositionTracker is a namespace
		positionTracker = new PositionTracker.PositionTracker(positionProvider, sensorProvider);
        positionTracker.IndoorMode = IsIndoor ();
	}

	// Starts recording player's position
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override void StartTrack() {
		base.StartTrack();
		positionTracker.StartTracking();
	}

	// Set the indoor mode
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override void SetIndoor(bool indoor) {
        base.SetIndoor (indoor);
        positionTracker.IndoorMode = indoor;
	}

	// Check if has GPS lock
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override Boolean HasLock() {
		//return positionTracker.HasPosition;
		//positionTracker only reports that it has a position after tracking has started. This function is used before that point.
		//Check status of unity location status instead.
//        log.info("HasLock status: " + Input.location.status);
		return (Input.location.status == LocationServiceStatus.Running);
	}
	
	// Stop tracking 
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override Track StopTrack() {
		base.StopTrack();
		positionTracker.StopTracking();
		positionTracker.Track.save(Platform.Instance.db);
		return positionTracker.Track;
	}

	// Reset GPS tracker
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override void Reset() {
		base.Reset();
		positionTracker.StartNewTrack();
	}

	public override void Update() {
		_time = positionTracker.ElapsedTime;
		_distance = positionTracker.ElapsedDistance;
		_pace = positionTracker.CurrentSpeed;
		if (positionTracker.HasPosition) {
			//UnityEngine.Debug.Log("Position Tracker has position");
			_position = positionTracker.CurrentPosition;
			//log the position to the HUD and console
			//UnityEngine.Debug.Log("PlayerPosition's position: " + _position.latitude + " " + _position.longitude);
			//DataVault.Set("sweat_points_unit", _position.latitude);
			//DataVault.Set("fps", _position.longitude);
		}
		else
		{
			//UnityEngine.Debug.LogWarning("Position Tracker Update - don't have position");
		}
		_bearing = positionTracker.CurrentBearing;


//		log.info("State: " + positionTracker.CurrentState + 
//                 ", speed: " + Pace + 
//                 ", distance: " + Distance + 
//                 ", time: " + Time);
		//UnityEngine.Debug.Log("Position: LinearAcceleration: " + sensorProvider.LinearAcceleration[0] + "," 
		//				+ sensorProvider.LinearAcceleration[1] + "," + sensorProvider.LinearAcceleration[2]);

		// Position Provider should arrange its own updates if it needs them. In the case of iOSPositionProvider, it's a MonoBehaviour so gets Unity's Update() call.
		//positionProvider.Update();
		sensorProvider.Update();
	}

	public override void NotifyAutoBearing() {
		// TODO: not in use for now
 	}

}
