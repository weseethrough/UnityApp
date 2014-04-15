using System;
using RaceYourself.Models;

namespace PositionTracker
{
	interface IPositionTracker
	{
	    void StartNewTrack();
		
		Track Track {
			get;
		}
	    
	    /**
	     * Start recording distance and time covered by the device.
	     * <p>
	     * Ideally this should only be called once the device has a GPS fix, which can be checked using
	     * hasPosition().
	     */
	    void StartTracking();
	
	    /**
	     * Stop recording distance and time covered by the device.
	     * <p>
	     * This will not reset the cumulative distance/time values, so it can be used to pause (e.g.
	     * when the user is stopped to do some stretches). Call startTracking() to continue from where
	     * we left off, or create a new GPSTracker object if a full reset is required.
	     */
	    void StopTracking();
	
		
		/*
	     * Is the GPS tracker currently recording the device's movement? See also startTracking() and
	     * stopTracking().
	     * 
	     * @return true if the device is recording elapsed distance/time and position data. False
	     *         otherwise.
	     */
	    bool Tracking  {
			get;
		}
		

	    /**
	     * Is the GPSTracker in indoor mode? If so, it'll fake GPS positions.
	     * 
	     * @return true if in indoor mode, false otherwise. Default is false.
	     */
	    bool IndoorMode  {
			get;set;
		}

		
		/**
	     * Calculates the device's current bearing based on the last few GPS positions. If unknown (e.g.
	     * the device is not moving) returns -999.0f.
	     * 
	     * @return bearing in degrees
	     */
	    float CurrentBearing  {
			get;
		}

		
		/**
	     * Returns the position of the device as a Position object, whether or not we are tracking.
	     * 
	     * @return position of the device
	     */
	    Position CurrentPosition();

		/**
	     * Returns the current speed of the device in m/s, or zero if we think we're stopped.
	     * 
	     * @return speed in m/s
	     */
	    float CurrentSpeed  {
			get;
		}
	    float SampleSpeed  {
			get;
		}


		/**
	     * Returns the distance covered by the device (in metres) since startTracking was called
	     * 
	     * @return Distance covered, in metres
	     */
	    double ElapsedDistance  {
			get;
		}
	    double SampleDistance  {
			get;
		}		

		/**
	     * Returns the cumulative time the isTracking() has been true. See also startTracking() and stopTracking(). 
	     * 
	     * @return cumulative time in milliseconds
	     */
	    long ElapsedTime {
			get;
		}
	}
}


