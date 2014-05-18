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
	private CrossPlatformPositionProvider positionProvider;
	private CrossPlatformSensorProvider sensorProvider;


	public CrossPlatformPlayerPosition() {
		positionProvider = new CrossPlatformPositionProvider();
		sensorProvider = new CrossPlatformSensorProvider();

		//Note - unity complains that PositionTracker is a namespace
		positionTracker = new PositionTracker.PositionTracker(positionProvider, sensorProvider);
	}

	// Starts recording player's position
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override void StartTrack() {
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
		UnityEngine.Debug.LogError("CrossPlatformPlayerPosition: Folder:  " +  Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));


		return (Input.location.status == LocationServiceStatus.Running);
	}
	
	// Stop tracking 
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override Track StopTrack() {
		UnityEngine.Debug.LogError("CrossPlatformPlayerPosition: Calling StopTrack  ");

		positionTracker.StopTracking();
		return positionTracker.Track;
	}

	// Reset GPS tracker
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override void Reset() {
		//positionTracker.Reset();

		//There doesn't appear to be a Reset method in the position tracker class.
		//Should we use StartNewTrack() instead?
		UnityEngine.Debug.LogError("CrossPlatformPlayerPosition: no reset method in PositionTracker. Use StartNewTrack()?");
		throw new NotImplementedException();
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

		positionProvider.Update();
		sensorProvider.Update();
	}

	public override void NotifyAutoBearing() {
		// TODO: not in use for now
 	}

}
