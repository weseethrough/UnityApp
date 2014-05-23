using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using RaceYourself.Models;
using PositionTracker;

public class CrossPlatformPlayerPosition : PlayerPosition {

	private Position _position = null;
	public override Position Position { get { return _position; } }

	private Position _predictedPosition = null;
	public override Position PredictedPosition { get { return _predictedPosition; } }

	private long _time = 0;
	public override long Time { get { return _time; } }

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
#if UNITY_IOS
		positionProvider = platform.AddComponent<IosPositionProvider>();
#endif
#if	UNITY_ANDROID
		PositionProvider = platform.AddComponent<CrossPlatformPositionProvider>();
#endif
		sensorProvider = new CrossPlatformSensorProvider();

		//Note - unity complains that PositionTracker is a namespace
		positionTracker = new PositionTracker.PositionTracker(positionProvider, sensorProvider);
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
		positionTracker.IndoorMode = indoor;
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public override bool IsIndoor() {
		return positionTracker.IndoorMode;
	}

	// Check if has GPS lock
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override Boolean HasLock() {
		//return positionTracker.HasPosition;
		//positionTracker only reports that it has a position after tracking has started. This function is used before that point.
		//Check status of unity location status instead.
		UnityEngine.Debug.LogError("CrossPlatformPlayerPosition: HasLock status: " + Input.location.status);

		return (Input.location.status == LocationServiceStatus.Running);
	}
	
	// Stop tracking 
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override Track StopTrack() {
		base.StopTrack();
		positionTracker.StopTracking();
		return positionTracker.Track;
	}

	// Reset GPS tracker
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override void Reset() {
		base.Reset();
		positionTracker.StartNewTrack();
		log.info("Starting new position track");
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
			UnityEngine.Debug.LogWarning("Position Tracker Update - don't have position");
		}
		_bearing = positionTracker.CurrentBearing;


		//UnityEngine.Debug.Log("Position: Position tracker's state: " + positionTracker.CurrentState + ", speed: " + positionTracker.CurrentSpeed);
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
