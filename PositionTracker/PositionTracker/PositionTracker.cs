using System;
using System.Diagnostics;
using System.Collections;

using RaceYourself.Models;
//using PositionPredictor;

namespace PositionTracker
{
	public class PositionTracker : IPositionTracker, IPositionListener
	{
		private ArrayList recentPositions = new ArrayList(10);
		
		private Track track;
		private Position gpsPosition;
		private bool isIndoorMode;
		//private float bearing;
		private float speed;
		private double elapsedDistance;
		private long elapsedTime;
		private State state = State.STOPPED;
		private bool isTracking;
				
		private Stopwatch trackStopwatch = new Stopwatch(); // time so far in milliseconds
	    private Stopwatch interpolationStopwatch = new Stopwatch(); // time so far in milliseconds
		
		private Position lastImportantPosition;
		
		private double gpsDistance;
		
		private PositionPredictor.PositionPredictor positionPredictor = new PositionPredictor.PositionPredictor();
		
		private static float MAX_TOLERATED_POSITION_ERROR = 21; // 21 metres
	    private static float EPE_SCALING = 0.5f; // if predicted positions lie within 0.5*EPE circle
                                                   // of reported GPS position, no need to store GPS pos.
		private static float INVALID_BEARING = -999.0f;


		
		public enum State {
			UNKNOWN,
			STOPPED,
			SENSOR_ACC,
			STEADY_GPS_SPEED,
			COAST,
			SENSOR_DEC
		}
		
		public void OnPositionUpdate(Position position) { 
	        // get the latest GPS position
	        //Position tempPosition = new Position(track, location);
	        //Log.i("GPSTracker", "New position with error " + tempPosition.getEpe());
	        
	        // if the latest gpsPosition doesn't meets our accuracy criteria, throw it away
	        if (position.epe > MAX_TOLERATED_POSITION_ERROR) {
	            return;
	        }       
	        
	        // update current position
	        // TODO: kalman filter to smooth GPS points?
	        Position lastPosition = gpsPosition;
	        gpsPosition = position;
	        
	        // stop here if we're not tracking
	        if (!isTracking) {
	            //broadcastToUnity();
	            return;
	        }
	        
	        // keep track of the pure GPS distance moved
	        if (lastPosition != null && state != State.STOPPED) {
	            // add dist between last and current position
	            // don't add distance if we're stopped, it's probably just drift 
	            gpsDistance += PositionUtils.distanceBetween(lastPosition, gpsPosition);
	        }
	        interpolationStopwatch.Reset();
	
	        // add position to the buffer for later use
	        if (recentPositions.Count >= 10) {
	            // if the buffer is full, discard the oldest element
	            recentPositions.RemoveAt(0);
	        }
	        recentPositions.Add(gpsPosition); //recentPositions.getLast() now points at gpsPosition.
	        
	        // calculate corrected bearing
	        // this is more accurate than the raw GPS bearing as it averages several recent positions
	        correctBearing(gpsPosition);
	        
	        // work out whether the position is important for recreating the track or
	        // if it could have been predicted from previous positions
	        // TODO: add checks for significant change in speed/bearing
			detectPositionImportance(gpsPosition, lastPosition);
	        
	        //gpsPosition.save(); // adds GUID
	        notifyPositionListeners(gpsPosition);
	        //sendToUnityAsJson(gpsPosition, "NewPosition");
	        //logPosition();

		
		}
	
		// calculate corrected bearing
	    // this is more accurate than the raw GPS bearing as it averages several recent positions
	    private void correctBearing(Position gpsPosition) {
	        // interpolate last few positions 
	        positionPredictor.updatePosition(gpsPosition);

			float? correctedBearing = positionPredictor.predictBearing(gpsPosition.device_ts);
	        if (correctedBearing != null) {
	          gpsPosition.corrected_bearing = (float)correctedBearing;
	        }    
	    }
		
		// work out whether the position is important for recreating the track or
	    // if it could have been predicted from previous positions
	    // TODO: add checks for significant change in speed/bearing
		private void detectPositionImportance(Position position, Position lastPosition) {
	        if (lastImportantPosition == null) {
	            // important position - first in track
	            position.state_id = (Int32)state;
	            lastImportantPosition = position;
	        } else if (Math.Abs(lastPosition.state_id) != (Int32)state) {
	            // change in state, positions either side of change are important
	            if (lastPosition.state_id < 0) lastPosition.state_id = (-1*lastPosition.state_id);
	            position.state_id = (Int32)state;
	            lastImportantPosition = position;
	        } else {
	            // no change in state, see if we could have predicted current position
	            Position predictedPosition = PositionUtils.predictPosition(lastImportantPosition, (position.device_ts - lastImportantPosition.device_ts));
	            if (predictedPosition == null || 
	                   	PositionUtils.distanceBetween(position, predictedPosition) > ((position.epe + 1) * EPE_SCALING)) {
	                // we cannot predict current position from the last important one
	                // mark the previous position as important (end of straight line) if not already
	                if (lastPosition.state_id < 0) {
	                    lastPosition.state_id = (-1*lastPosition.state_id);
	                    lastImportantPosition = position;
	                }
	                // try to predict current position again (from the new lastImportantPosition)
	                predictedPosition = PositionUtils.predictPosition(lastImportantPosition, (position.device_ts - lastImportantPosition.device_ts));
	                if (predictedPosition == null || 
	                        PositionUtils.distanceBetween(position, predictedPosition) > ((position.epe + 1) * EPE_SCALING)) {
	                    // error still too big (must be sharp corner, not gradual curve) so mark this one as important too
	                    position.state_id = (Int32)state;
	                    lastImportantPosition = position;
	                }
	            } else {
	                // not important, we could have predicted it
	                position.state_id = (-1*(Int32)state);
	            }
	        }
			
		}
		
		private void notifyPositionListeners(Position position) {
			// TODO: notify listerens on new position
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
	    
	    
	    public void StartNewTrack() {		
		    trackStopwatch.Stop();
	        trackStopwatch.Reset();
	        interpolationStopwatch.Stop();
	        interpolationStopwatch.Reset();
	        isTracking = false;
	        elapsedDistance = 0.0;
	        gpsDistance = 0.0;
	        speed = 0.0f;
	        state = State.STOPPED;
	        recentPositions.Clear();
	        
	        track = null;

		}
		
		public Track Track {
	        get { return track; }
	    }
	    
		
	    public Position CurrentPosition() {
	        return gpsPosition;
	    }
	
		public bool HasPosition() {
			return (gpsPosition != null);
		}
		
	
	    /**
	     * Start recording distance and time covered by the device.
	     * <p>
	     * Ideally this should only be called once the device has a GPS fix, which can be checked using
	     * hasPosition().
	     */
	    public void StartTracking() {
			if (track == null) {
				track = new Track();
				track.trackName = "Test";
				// TODO: track user id? save?
			}
			// Set track for temporary position
        	if (gpsPosition != null) gpsPosition.trackId = track.trackId;
        
        	// if we already have a position, start the stopwatch, if not it'll
        	// be triggered when we get our first decent GPS fix
        	if (HasPosition()) {
            	trackStopwatch.Start();
            	interpolationStopwatch.Start();
        	}
	        isTracking = true;

		}
	
	    /**
	     * Stop recording distance and time covered by the device.
	     * <p>
	     * This will not reset the cumulative distance/time values, so it can be used to pause (e.g.
	     * when the user is stopped to do some stretches). Call startTracking() to continue from where
	     * we left off, or create a new GPSTracker object if a full reset is required.
	     */
	    public void StopTracking() {
		    isTracking = false;
	        if (track != null) {
	            track.distance = elapsedDistance;
	            track.time = trackStopwatch.ElapsedMilliseconds;
	            track.track_type_id = ((this.IndoorMode ? -1 : 1) * 2); //negative if indoor
	            //track.save();
	        }
	        trackStopwatch.Stop();
	        interpolationStopwatch.Stop();
	        positionPredictor.stopTracking();

		
		}
	    
		/**
	     * Is the GPS tracker currently recording the device's movement? See also startTracking() and
	     * stopTracking().
	     * 
	     * @return true if the device is recording elapsed distance/time and position data. False
	     *         otherwise.
	     */
	    public bool Tracking {
	        get { return isTracking; }
	    }
		
	    /**
	     * Is the GPSTracker in indoor mode? If so, it'll fake GPS positions.
	     * 
	     * @return true if in indoor mode, false otherwise. Default is false.
	     */
	    public bool IndoorMode {
	        get { return isIndoorMode;  }
		    // indoorMode == false => Listen for real GPS positions
	        // indoorMode == true => Generate fake GPS positions
			set { isIndoorMode = value; }
	    }
	
	    
	    /**
	     * Calculates the device's current bearing based on the last few GPS positions. If unknown (e.g.
	     * the device is not moving) returns -999.0f.
	     * 
	     * @return bearing in degrees
	     */
		// TODO: make functions instead of property?
	    public float CurrentBearing {
			get { 
				 float? bearing = positionPredictor.predictBearing(currentTimeMillis());
        		 if (bearing != null) {
            		return (float)bearing;
        		} else {
            		return INVALID_BEARING;
        		}
			}
	    }

	
	    /**
	     * Returns the current speed of the device in m/s, or zero if we think we're stopped.
	     * 
	     * @return speed in m/s
	     */
	    public float CurrentSpeed {
	        get { return speed; }
	    }
		
		public float SampleSpeed {
	        get { return 0.0f; }
	    }
		
		
	    /**
	     * Returns the distance covered by the device (in metres) since startTracking was called
	     * 
	     * @return Distance covered, in metres
	     */
	    public double ElapsedDistance {
	        get { return elapsedDistance; }
	    }
	    public double SampleDistance {
	        get { return 0.0;}
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
	    public long ElapsedTime {
	        get { return this.elapsedTime; }
	    }
		
		private long currentTimeMillis() {
			return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		}

	}
	

}
