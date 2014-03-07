using System;
using UnityEngine;
using System.Runtime.CompilerServices;

public abstract class PlayerPosition {

	public abstract Position Position { get; }
	public abstract long Time { get; }
	public abstract double Distance { get; }
	public int Calories { get { return (int)(76.0 / 1000.0 * Distance); } }
	public abstract float Pace { get; }
	public abstract float Bearing { get; }

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
		this.indoor = indoor;
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual bool IsIndoor()
	{
		return indoor;
	}

	// Check if has GPS lock
	[MethodImpl(MethodImplOptions.Synchronized)]
	public abstract Boolean HasLock();

}