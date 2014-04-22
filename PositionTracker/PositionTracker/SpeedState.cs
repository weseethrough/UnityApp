using System;

namespace PositionTracker
{

	public class SpeedState {
    	private float ACCELERATE_THRESHOLD = 0.45f;
        private float DECELERATE_THRESHOLD = 0.35f;
		
		private PositionTracker.State currentState;
		
		public PositionTracker.State CurrentState { 
			get { return currentState; }
			set { currentState = value; SaveEntryTime(); }
		}
		
		public PositionTracker.State NextState(float rmsForwardAcc, float gpsSpeed) {
			switch(CurrentState) {
			case PositionTracker.State.UNKNOWN:
				return CurrentState; 
				
			case PositionTracker.State.STOPPED:
				CurrentState = nextStateStopped(rmsForwardAcc, gpsSpeed);
				break;
				
			case PositionTracker.State.SENSOR_ACC:
				CurrentState =  nextStateSensorAcc(rmsForwardAcc, gpsSpeed);
				break;

			case PositionTracker.State.STEADY_GPS_SPEED:
				CurrentState =  nextStateSteadyGpsSpeed(rmsForwardAcc, gpsSpeed);
				break;

			case PositionTracker.State.COAST:
				CurrentState =  nextStateCoast(rmsForwardAcc, gpsSpeed);
				break;

			case PositionTracker.State.SENSOR_DEC:
				CurrentState =  nextStateSensorDec(rmsForwardAcc, gpsSpeed);
				break;

			}
			return CurrentState;
		}
		
		public long EntryTime { get; set; }
		
		private void SaveEntryTime() {
			EntryTime = Utils.CurrentTimeMillis();
		}
		
		private long GetTimeInState() {
        	return (Utils.CurrentTimeMillis() - EntryTime);
    	}

		private PositionTracker.State nextStateStopped(float rmsForwardAcc, float gpsSpeed) {
			if (rmsForwardAcc > ACCELERATE_THRESHOLD) {
                //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","SENSOR_ACC");
				SaveEntryTime();
                return PositionTracker.State.SENSOR_ACC;
			}
    		return CurrentState;
    	}

		private PositionTracker.State nextStateSensorAcc(float rmsForwardAcc, float gpsSpeed) {
			if (gpsSpeed > 0.0f) {
                //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","STEADY_GPS_SPEED");
				SaveEntryTime();
                return PositionTracker.State.STEADY_GPS_SPEED;
            } else if (rmsForwardAcc < DECELERATE_THRESHOLD) {
                //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","SENSOR_DEC");
				SaveEntryTime();
                return PositionTracker.State.SENSOR_DEC;
            } 
    		return CurrentState;
    	}		
		
		private PositionTracker.State nextStateSteadyGpsSpeed(float rmsForwardAcc, float gpsSpeed) {
            if (rmsForwardAcc < DECELERATE_THRESHOLD) {
                // if the sensors suggest the device has stopped moving, decelerate
                // TODO: pick up when we're in a tunnel and need to coast
                //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","SENSOR_DEC");
				SaveEntryTime();
                return PositionTracker.State.SENSOR_DEC;
			} else if (gpsSpeed == 0.0f) {
                // if we've picked up a dodgy GPS position, maintain const speed
                //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","COAST");
				SaveEntryTime();
                return PositionTracker.State.COAST;
            }                
    		return CurrentState;
    	}			
		
		private PositionTracker.State nextStateCoast(float rmsForwardAcc, float gpsSpeed) {
			if (rmsForwardAcc < DECELERATE_THRESHOLD) {
                // if sensors suggest the device has stopped moving, decelerate
                //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","SENSOR_DEC");
				SaveEntryTime();
                return PositionTracker.State.SENSOR_DEC;
            } else if (gpsSpeed > 0.0f) {
                // we've picked up GPS again
                //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","STEADY_GPS_SPEED");
				SaveEntryTime();
                return PositionTracker.State.STEADY_GPS_SPEED;
            }
    		return CurrentState;
    	}			
		
		private PositionTracker.State nextStateSensorDec(float rmsForwardAcc, float gpsSpeed) {
            if (gpsSpeed == 0.0f) {
                //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","STOPPED");
				SaveEntryTime();
                return PositionTracker.State.STOPPED;
            } else if (GetTimeInState() > 3000) {
                //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","STEADY_GPS_SPEED");
				SaveEntryTime();
                return PositionTracker.State.STEADY_GPS_SPEED;
            } else if (rmsForwardAcc > ACCELERATE_THRESHOLD) {
                //UnityInterface.unitySendMessage("Platform", "PlayerStateChange","SENSOR_ACC");
				SaveEntryTime();
                return PositionTracker.State.SENSOR_ACC;
            }				
    		return CurrentState;
    	}			
		
	}
}

