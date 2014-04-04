using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using RaceYourself.Models;

#if UNITY_IPHONE
public class IosPlayerPosition : PlayerPosition {

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


    public IosPlayerPosition() {
        log.info ("Connecting to Ios GPS");
        // TODO: get iOS to start serching for GPS
        throw new NotImplementedException ();
    }

    // Starts recording player's position
    [MethodImpl(MethodImplOptions.Synchronized)]
    public override void StartTrack() {
        // TODO: Start recording user's track
        throw new NotImplementedException();
    }

    // Set the indoor mode
    [MethodImpl(MethodImplOptions.Synchronized)]
    public override void SetIndoor(bool indoor) {
        // TODO: Start using fake data. StopTrack searching for GPS. And vice-versa.
        throw new NotImplementedException ();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public override bool IsIndoor() {
        // TODO: return true if using fake data, false if using GPS data
        throw new NotImplementedException ();
    }

    // Check if has GPS lock
    [MethodImpl(MethodImplOptions.Synchronized)]
    public override Boolean HasLock() {
        // TODO: return true if we have a *good* GPS lock, false otherwise
        throw new NotImplementedException (); 
    }

    // Stop tracking 
    [MethodImpl(MethodImplOptions.Synchronized)]
    public override Track StopTrack() {
        // TODO: stop tracking
        throw new NotImplementedException();
    }

    // Reset GPS tracker
    [MethodImpl(MethodImplOptions.Synchronized)]
    public override void Reset() {
        base.Reset();
        // TODO: stop tracking, reset dist to zero, start new track
        throw new NotImplementedException();
    }

    public override void Update() {
        // TODO: update position, speed, distance variables
        //       usually called once per frame during a workout
        throw new NotImplementedException();
    }

    public override void NotifyAutoBearing() {
    }

}
#endif