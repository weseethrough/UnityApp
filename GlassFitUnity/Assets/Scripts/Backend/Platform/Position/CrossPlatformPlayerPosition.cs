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
		return positionTracker.HasPosition;
	}
	
	// Stop tracking 
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override Track StopTrack() {
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
			_position = positionTracker.CurrentPosition;
		}
		_bearing = positionTracker.CurrentBearing;

		sensorProvider.Update();
	}

	public override void NotifyAutoBearing() {
		// TODO: not in use for now
 	}

}
