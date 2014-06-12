using System;
using UnityEngine;
using System.Runtime.CompilerServices;

using RaceYourself.Models;

public abstract class PlayerPosition {

	public abstract Position Position { get; }
	public abstract Position PredictedPosition { get; }
	public abstract long Time { get; }
	public abstract double Distance { get; }
	public int Calories { get { return (int)(76.0 / 1000.0 * Distance); } }
	public abstract float Pace { get; }
	public abstract float Bearing { get; }

    internal string playerState = "";  // Player state - STOPPED, STEADY_GPS_SPEED etc. Set from Java via PositionMessageListener;
    internal float playerStateEntryTime = UnityEngine.Time.time;

	private bool _tracking = false;
	public bool IsTracking { get { return _tracking; } }

	private bool started = false;
	private bool indoor = false;

	protected Log log = new Log("PlayerPosition");  // for use by subclasses

	public abstract void Update();

	// Starts recording player's position
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual void StartTrack() {
		_tracking = true;
		started = true;
		log.info("Started tracking");
	}

	// Stop tracking
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual Track StopTrack() {
		_tracking = false;
		started = false;
		log.info("Stopped tracking");
		return null;
	}

	// Reset GPS tracker
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual void Reset() {
		_tracking = false;
		started = false;
		log.info("Reset track");
	}

	// Set the indoor mode
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual void SetIndoor(bool indoor)
	{
		this.indoor = true;
		//this.indoor = indoor;
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual bool IsIndoor()
	{
		return true;
		//return indoor;
	}

	// Check if has GPS lock
	[MethodImpl(MethodImplOptions.Synchronized)]
	public abstract Boolean HasLock();

	[MethodImpl(MethodImplOptions.Synchronized)]
	public abstract void NotifyAutoBearing();

    public void SetPlayerState(String state) {
        playerState = state;
        playerStateEntryTime = UnityEngine.Time.time;
    }

}