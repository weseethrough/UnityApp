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
		//private float bearing;
		private State state = State.STOPPED;
				
		private Stopwatch trackStopwatch = new Stopwatch(); // time so far in milliseconds
	    private Stopwatch interpolationStopwatch = new Stopwatch(); // time so far in milliseconds
		
		private Position lastImportantPosition;
				
		private PositionPredictor.PositionPredictor positionPredictor = new PositionPredictor.PositionPredictor();
		
		private static float MAX_TOLERATED_POSITION_ERROR = 21; // 21 metres
	    private static float EPE_SCALING = 0.5f; // if predicted positions lie within 0.5*EPE circle
                                                   // of reported GPS position, no need to store GPS pos.
		private static float INVALID_BEARING = -999.0f;
		// time in milliseconds over which current position will converge with the
    	// more accurate but non-continuous extrapolated GPS position
    	private static long DISTANCE_CORRECTION_MILLISECONDS = 1500; 



		
		public enum State {
			UNKNOWN,
			STOPPED,
			SENSOR_ACC,
			STEADY_GPS_SPEED,
			COAST,
			SENSOR_DEC
		}
		
		PositionTracker() {
			MinIndoorSpeed = 0.0f;
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
	        if (!Tracking) {
	            //broadcastToUnity();
	            return;
	        }
	        
	        // keep track of the pure GPS distance moved
	        if (lastPosition != null && state != State.STOPPED) {
	            // add dist between last and current position
	            // don't add distance if we're stopped, it's probably just drift 
	            GpsDistance += PositionUtils.distanceBetween(lastPosition, gpsPosition);
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
		private void notifyPositionListeners(State newState) {
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
	        Tracking = false;
	        ElapsedDistance = 0.0;
	        GpsDistance = 0.0;
	        CurrentSpeed = 0.0f;
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
	        Tracking = true;

		}
	
	    /**
	     * Stop recording distance and time covered by the device.
	     * <p>
	     * This will not reset the cumulative distance/time values, so it can be used to pause (e.g.
	     * when the user is stopped to do some stretches). Call startTracking() to continue from where
	     * we left off, or create a new GPSTracker object if a full reset is required.
	     */
	    public void StopTracking() {
		    Tracking = false;
	        if (track != null) {
	            track.distance = ElapsedDistance;
	            track.time = trackStopwatch.ElapsedMilliseconds;
	            track.track_type_id = ((IndoorMode ? -1 : 1) * 2); //negative if indoor
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
	        get; set;
	    }
		
	    /**
	     * Is the GPSTracker in indoor mode? If so, it'll fake GPS positions.
	     * 
	     * @return true if in indoor mode, false otherwise. Default is false.
	     */
	    public bool IndoorMode {
			get; set;
			// indoorMode == false => Listen for real GPS positions
	        // indoorMode == true => Generate fake GPS positions
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
	        get; set;
	    }
		
		public float SampleSpeed {
	        get; set;
	    }
		
		public float GpsSpeed {
	        get { return (gpsPosition == null) ? 0.0f :gpsPosition.speed ; }
	    }
		
		public float MaxIndoorSpeed {
	        get; set;
	    }
		public float MinIndoorSpeed {
	        get; set;
	    }
	    /**
	     * Returns the distance covered by the device (in metres) since startTracking was called
	     * 
	     * @return Distance covered, in metres
	     */
	    public double ElapsedDistance {
	        get; set;
	    }
	    public double SampleDistance {
	        get; set;
	    }
		public double GpsDistance {
	        get; set;
	    }
		public double ExtrapolatedGpsDistance {
	        get; set;
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
	        get; set;
	    }
		
		private static long currentTimeMillis() {
			return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		}
		
		
		public class Tick {

	        private long tickTime;
	        private long lastTickTime;
	        private float gpsSpeed = 0.0f;
	        private float lastForwardAcc = 0.0f;
	        private float lastTotalAcc = 0.0f;
	        private DescriptiveStatistics dFaStats = new DescriptiveStatistics(10);
	        private DescriptiveStatistics dTaStats = new DescriptiveStatistics(10);
	        private DescriptiveStatistics taStats = new DescriptiveStatistics(10);
	
			private ISensorProvider sensorProvider;
			private PositionTracker positionTracker;
			
			public Tick(PositionTracker positionTracker, ISensorProvider sensorProvider) {
				this.positionTracker = positionTracker;
				this.sensorProvider = sensorProvider;
			}
			
	        public void Run() {
	            
	            if (lastTickTime == 0) {
	                lastTickTime = currentTimeMillis();
	                return;
	            }
	            tickTime = currentTimeMillis();
	
	            // update buffers with most recent sensor sample
	            dFaStats.AddValue(Math.Abs(sensorProvider.ForwardAcceleration-lastForwardAcc));
	//            rmsForwardAcc = (float)Math.sqrt(0.95*Math.pow(rmsForwardAcc,2) + 0.05*Math.pow(getForwardAcceleration(),2));
	            taStats.AddValue(sensorProvider.TotalAcceleration);
	            dTaStats.AddValue(Math.Abs(sensorProvider.TotalAcceleration-lastTotalAcc));
	            
	            // compute some stats on the buffers
	            // TODO: frequency analysis
	            float meanDfa = (float)dFaStats.Mean;
	            float meanDta = (float)dTaStats.Mean;
	            float meanTa = (float)taStats.Mean;
	            float maxDta = (float)dTaStats.Maximum;
	            float sdTotalAcc = (float)taStats.StandardDeviation;
	            gpsSpeed = positionTracker.GpsSpeed;
	            
	            // update state
	            // gpsSpeed = -1.0 for indoorMode to prevent entry into
	            // STEADY_GPS_SPEED (just want sensor_acc/dec)
	            State lastState = positionTracker.state;
	            positionTracker.state = positionTracker.nextState(meanDta, (positionTracker.IndoorMode ? -1.0f : gpsSpeed));
	            
	            // save for next loop
	            lastForwardAcc = sensorProvider.ForwardAcceleration;
	            lastTotalAcc = sensorProvider.TotalAcceleration;
	            
				float outdoorSpeed = positionTracker.CurrentSpeed;
	            // adjust speed
	            switch (positionTracker.state) {
	                case State.STOPPED:
	                    // speed is zero!
	                    outdoorSpeed = 0.0f;
	                    break;
	                case State.SENSOR_ACC:
	                    // increase speed at 1.0m/s/s (typical walking acceleration)
	                    float increment = 1.0f * (tickTime - lastTickTime) / 1000.0f;
	
	                    // cap speed at some sensor-driven speed, and up to maxIndoorSpeed indoors
	                    // TODO: freq analysis to more accurately identify speed
	                    float sensorSpeedCap = meanTa;
	                    if (positionTracker.IndoorMode && sensorSpeedCap > positionTracker.MaxIndoorSpeed) sensorSpeedCap = positionTracker.MaxIndoorSpeed;
	                    
	                    if (outdoorSpeed < sensorSpeedCap) {
	                        // accelerate
	                        outdoorSpeed += increment;
	                    } else if (outdoorSpeed > 0) {
	                        // decelerate
	                        outdoorSpeed -= increment;
	                    }
	                    break;
	                case State.STEADY_GPS_SPEED:
	                    // smoothly adjust speed toward the GPS speed
	                    // TODO: maybe use acceleration sensor here to make this more responsive?
	                    outdoorSpeed = 0.9f * outdoorSpeed + 0.1f * gpsSpeed;
	                    break;
	                case State.COAST:
	                    // maintain constant speed
	                    break;
	                case State.SENSOR_DEC:
	                    // decrease speed at 2.0 m/s/s till we are stopped (or 
	                    // minIndoorSpeed in indoorMode)
	                    float decrement = 2.0f * (tickTime - lastTickTime) / 1000.0f;
	                    if (outdoorSpeed -decrement > (positionTracker.IndoorMode ? positionTracker.MinIndoorSpeed : 0.0f)) {
	                        outdoorSpeed -= decrement;
	                    } else {
	                        outdoorSpeed = (positionTracker.IndoorMode ? positionTracker.MinIndoorSpeed : 0.0f);
	                    }
	                    break;
	            }
				positionTracker.CurrentSpeed = outdoorSpeed;
	            // update distance travelled
	            if (positionTracker.Tracking) {
	                
	                // extrapolate distance based on last known fix + outdoor speed
	                // accurate and responsive, but not continuous (i.e. avatar would 
	                // jump backwards/forwards each time a new fix came in)
	                positionTracker.ExtrapolatedGpsDistance = positionTracker.GpsDistance
	                        + outdoorSpeed * (positionTracker.interpolationStopwatch.ElapsedMilliseconds) / 1000.0;
	                
	                // calculate the speed we need to move at to make
	                // distanceTravelled converge with extrapolatedGpsDistance over
	                // a period of DISTANCE_CORRECTION_MILLISECONDS
	                double correctiveSpeed = outdoorSpeed + 
	                        (positionTracker.ExtrapolatedGpsDistance - positionTracker.ElapsedDistance) * 1000.0 / DISTANCE_CORRECTION_MILLISECONDS;
	                
	                // increment distance traveled by camera at this new speed
	                positionTracker.ElapsedDistance += correctiveSpeed * (tickTime - lastTickTime) / 1000.0;
	                
	                if (positionTracker.state != lastState) positionTracker.notifyPositionListeners(positionTracker.state);
	                
	            }
	            
	            lastTickTime = tickTime;
	        }
    	}
		
		private State nextState(float rmsForwardAcc, float gpsSpeed) {
        	return SpeedState.NextState(state, rmsForwardAcc, gpsSpeed);
        }
		
		
		private class SpeedState {
			private static State currentState;
        	private static float ACCELERATE_THRESHOLD = 0.45f;
            private static float DECELERATE_THRESHOLD = 0.35f;

			public static State NextState(State state, float rmsForwardAcc, float gpsSpeed) {
				currentState = state; 
				switch(currentState) {
				case State.UNKNOWN:
					return currentState; 
					
				case State.STOPPED:
					return nextStateStopped(rmsForwardAcc, gpsSpeed);
	
				case State.SENSOR_ACC:
					return nextStateSensorAcc(rmsForwardAcc, gpsSpeed);

				case State.STEADY_GPS_SPEED:
					return nextStateSteadyGpsSpeed(rmsForwardAcc, gpsSpeed);
					
				case State.COAST:
					return nextStateCoast(rmsForwardAcc, gpsSpeed);
					
				case State.SENSOR_DEC:
					return nextStateSensorDec(rmsForwardAcc, gpsSpeed);
				}
				return currentState;
			}
			
			private static long EntryTime { get; set; }
			
			private static void SaveEntryTime() {
				EntryTime = PositionTracker.currentTimeMillis();
			}
			
			private static long GetTimeInState() {
            	return (PositionTracker.currentTimeMillis() - EntryTime);
        	}

			private static State nextStateStopped(float rmsForwardAcc, float gpsSpeed) {
				if (rmsForwardAcc > ACCELERATE_THRESHOLD) {
                    //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","SENSOR_ACC");
					SaveEntryTime();
                    return State.SENSOR_ACC;
				}
        		return currentState;
        	}

			private static State nextStateSensorAcc(float rmsForwardAcc, float gpsSpeed) {
				if (gpsSpeed > 0.0f) {
                    //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","STEADY_GPS_SPEED");
					SaveEntryTime();
                    return State.STEADY_GPS_SPEED;
                } else if (rmsForwardAcc < DECELERATE_THRESHOLD) {
                    //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","SENSOR_DEC");
					SaveEntryTime();
                    return State.SENSOR_DEC;
                } 
        		return currentState;
        	}		
			
			private static State nextStateSteadyGpsSpeed(float rmsForwardAcc, float gpsSpeed) {
                if (rmsForwardAcc < DECELERATE_THRESHOLD) {
                    // if the sensors suggest the device has stopped moving, decelerate
                    // TODO: pick up when we're in a tunnel and need to coast
                    //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","SENSOR_DEC");
					SaveEntryTime();
                    return State.SENSOR_DEC;
				} else if (gpsSpeed == 0.0f) {
                    // if we've picked up a dodgy GPS position, maintain const speed
                    //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","COAST");
					SaveEntryTime();
                    return State.COAST;
                }                
        		return currentState;
        	}			
			
			private static State nextStateCoast(float rmsForwardAcc, float gpsSpeed) {
				if (rmsForwardAcc < DECELERATE_THRESHOLD) {
                    // if sensors suggest the device has stopped moving, decelerate
                    //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","SENSOR_DEC");
					SaveEntryTime();
                    return State.SENSOR_DEC;
                } else if (gpsSpeed > 0.0f) {
                    // we've picked up GPS again
                    //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","STEADY_GPS_SPEED");
					SaveEntryTime();
                    return State.STEADY_GPS_SPEED;
                }
        		return currentState;
        	}			
			
			private static State nextStateSensorDec(float rmsForwardAcc, float gpsSpeed) {
                if (gpsSpeed == 0.0f) {
                    //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","STOPPED");
					SaveEntryTime();
                    return State.STOPPED;
                } else if (GetTimeInState() > 3000) {
                    //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","STEADY_GPS_SPEED");
					SaveEntryTime();
                    return State.STEADY_GPS_SPEED;
                } else if (rmsForwardAcc > ACCELERATE_THRESHOLD) {
                    //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","SENSOR_ACC");
					SaveEntryTime();
                    return State.SENSOR_ACC;
                }				
        		return currentState;
        	}			
			
		}
	}

}

