using System;
using RaceYourself.Models;

namespace PositionTracker
{
	public class PositionTracker
	{
		public enum State {
			UNKNOWN,
			STOPPED,
			SENSOR_ACC,
			STEADY_GPS_SPEED,
			COAST,
			SENSOR_DEC
		}
			
	
	    /**
	     * Starts / re-starts the methods that poll GPS and sensors.
	     * This is NOT an override - needs calling manually by containing Activity on app. resume
	     * Safe to call repeatedly
	     * 
	     */
	    public void OnResume() { }
	
	    /**
	     * Stops the methods that poll GPS and sensors
	     * This is NOT an override - needs calling manually by containing Activity on app. resume
	     * Safe to call repeatedly
	     */
	    public void OnPause() {}
	    
	    
	    public void StartNewTrack() { }
		
		public Track Track {
	        get { return null; }
	    }
	    
	
	    /**
	     * Returns the position of the device as a Position object, whether or not we are tracking.
	     * 
	     * @return position of the device
	     */
	    public Position GpsPosition() {
	        return null;
	    }
	
	    public Position PredictedPosition() {
	        return null;
	    }
	
	
	    /**
	     * Start recording distance and time covered by the device.
	     * <p>
	     * Ideally this should only be called once the device has a GPS fix, which can be checked using
	     * hasPosition().
	     */
	    public void StartTracking() {  }
	
	    /**
	     * Stop recording distance and time covered by the device.
	     * <p>
	     * This will not reset the cumulative distance/time values, so it can be used to pause (e.g.
	     * when the user is stopped to do some stretches). Call startTracking() to continue from where
	     * we left off, or create a new GPSTracker object if a full reset is required.
	     */
	    public void StopTracking() {   }
	    
		/**
	     * Is the GPS tracker currently recording the device's movement? See also startTracking() and
	     * stopTracking().
	     * 
	     * @return true if the device is recording elapsed distance/time and position data. False
	     *         otherwise.
	     */
	    public bool Tracking {
	        get { return false; }
	    }
		
	    /**
	     * Is the GPSTracker in indoor mode? If so, it'll fake GPS positions.
	     * 
	     * @return true if in indoor mode, false otherwise. Default is false.
	     */
	    public bool IndoorMode {
	        get { return false;  }
		    // indoorMode == false => Listen for real GPS positions
	        // indoorMode == true => Generate fake GPS positions
			set { }
	    }
	

	    /**
	     * Sets the speed for indoor mode to the supplied float value,
	     * measured in m/s. See also isIndoorMode().
	     * 
	     * @param indoorSpeed in m/s
	     */
	    public float IndoorSpeed {
	        set { }
	    }    
	    
	    /**
	     * Calculates the device's current bearing based on the last few GPS positions. If unknown (e.g.
	     * the device is not moving) returns -999.0f.
	     * 
	     * @return bearing in degrees
	     */
	    public float CurrentBearing {
			get { return 0.0f; }
	    }

		// Notify position tracker about auto-bearing reset in UI
	    public void NotifyAutoBearing(float autoBearing) {
	    }
	    
	
	    /**
	     * Returns the current speed of the device in m/s, or zero if we think we're stopped.
	     * 
	     * @return speed in m/s
	     */
	    public float CurrentSpeed {
	        get { return 0.0f; }
	    }

		public float Yaw {
			get { return 0.0f; }
		}

		public float ForwardAcceleration {
			get { return 0.0f; }
		}

		public float TotalAcceleration {
			get { return 0.0f; }
		}
		
	    /**
	     * Returns the distance covered by the device (in metres) since startTracking was called
	     * 
	     * @return Distance covered, in metres
	     */
	    public double ElapsedDistance {
	        get { return 0.0;}
	    }
	    
	    public double GpsDistance {
	        get { return 0.0; }
	    }
	    
	    public float GpsSpeed {
	        get { return 0.0f; }
	    }
	    
		// TODO:
	    public State CurrentState {
	        get { return State.UNKNOWN; }
	    }
	
	    /**
	     * Returns the cumulative time the isTracking() has been true. See also startTracking() and stopTracking(). 
	     * 
	     * @return cumulative time in milliseconds
	     */
	    public long ElapsedTime() {
	        return 0;
	    }
	    
	    
	    
	    private float meanDfa = 0.0f; // mean acceleration change in the forward-backward axis
	    private float meanDta = 0.0f; // mean acceleration change (all axes combined)
	    private float meanTa = 0.0f; // mean acceleration (all axes combined)
	    private float sdTotalAcc = 0.0f; // std deviation of total acceleration (all axes)
	    private float maxDta = 0.0f; // max change in total acceleration (all axes)
	    private double extrapolatedGpsDistance = 0.0; // extraploated distance travelled (based on sensors) to add to GPS distance
	    
	    public float MeanDfa {
	        get { return meanDfa; }
	    }

		public float MeanDta {
	        get { return meanDta; }
	    }
		
	    public float SdTotalAcc {
	        get { return sdTotalAcc; }
	    }

	    public float MaxDta {
	        get { return maxDta; }
	    }
		
	    public float ExtrapolatedGpsDistance {
	        get { return (float)extrapolatedGpsDistance; }
	    }
		
	    

	    

	}
}

