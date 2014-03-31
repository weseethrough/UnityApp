using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using RaceYourself.Models;
#if UNITY_ANDROID
public class AndroidPlayerPosition : PlayerPosition {

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

	private AndroidJavaObject currentActivity;
	private AndroidJavaObject helper;
	private AndroidJavaObject androidGpsTracker;


	public AndroidPlayerPosition() {

		log.info ("Connecting to Android GPS");
		try {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    		currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
				AndroidJavaClass helper_class = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper");
                helper = helper_class.CallStatic<AndroidJavaObject>("getInstance", context);
                androidGpsTracker = helper.Call<AndroidJavaObject>("getGPSTracker");
                log.info("Connected to Android GPS");
	    	}));

	    } catch (Exception e) {
            log.error(e, "Error connecting to Android GPS");
            log.exception(e);
			Application.Quit();
	    }
	}

	// Starts recording player's position
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override void StartTrack() {
		try {
			androidGpsTracker.Call("startTracking");
			base.StartTrack();
			log.info("StartTrack succeeded");
		} catch (Exception e) {
			log.warning("StartTrack failed " + e.Message);
			log.exception(e);
		}
	}

	// Set the indoor mode
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override void SetIndoor(bool indoor) {
		try {
			log.info("setting indoor");
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    	    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			
			currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
				androidGpsTracker.Call("setIndoorMode", indoor);
				if (indoor) {
				    androidGpsTracker.Call("setIndoorSpeed", 5.0f);
				    log.info("Indoor mode set to true, indoor speed = 4 min/km");
				} else {
					log.info("Indoor mode set to false, will use true GPS speed");
				}
			}));
		} catch(Exception e) {
			log.info("Error setting indoor mode " + e.Message);
		}
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public override bool IsIndoor() {
  		try {
			//log.info("checking indoor");
		    return androidGpsTracker.Call<bool>("isIndoorMode");
		  } catch (Exception e) {
		   log.warning("Error returning isIndoor");
		   log.info(e.Message);
		   return false;
		  }
	}

	// Check if has GPS lock
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override Boolean HasLock() {
		try {
			//log.info("checking GPS");
			bool gpsLock = androidGpsTracker.Call<Boolean>("hasPosition");
//			log.info("hasLock() returned " + gpsLock);
			return gpsLock;
		} catch (Exception e) {
			log.warning("hasLock() failed: " + e.Message);
			log.exception(e);
			return false;
		}
	}
	
	// Stop tracking 
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override Track StopTrack() {
		try {
			androidGpsTracker.Call("stopTracking");
			base.StopTrack();
			using (AndroidJavaObject rawtrack = androidGpsTracker.Call<AndroidJavaObject>("getTrack")) {
				return new AndroidTrack(rawtrack);
			}
		} catch(Exception e) {
			log.warning("Problem stopping tracking");
			log.exception(e);
			return null;
		}
	}

	// Reset GPS tracker
	[MethodImpl(MethodImplOptions.Synchronized)]
	public override void Reset() {
		base.Reset();
		try {
			androidGpsTracker.Call("startNewTrack");
			Update();
			Platform.Instance.PlayerPoints.Reset();
			log.info("GPS has been reset");
		} catch (Exception e) {
			log.warning("reset() failed: " + e.Message);
			log.exception(e);
		}
	}

	public override void Update() {

		try {
			_time = androidGpsTracker.Call<long>("getElapsedTime");
			//log.info("poll time");
		} catch (Exception e) {
			log.warning("getElapsedTime() failed: " + e.Message);
			log.exception(e);
		}
		
		try {
			_distance = androidGpsTracker.Call<double>("getElapsedDistance");
			//log.info("poll distance");
		} catch (Exception e) {
			log.warning("getElapsedDistance() failed: " + e.Message);
			log.exception(e);
		}
		try {
			_pace = androidGpsTracker.Call<float>("getCurrentSpeed");
			//log.info("poll speed");
		} catch (Exception e) {
			log.warning("getCurrentSpeed() failed: " + e.Message);
			log.exception(e);
		}
		try {
			if (HasLock()) {
				AndroidJavaObject ajo = androidGpsTracker.Call<AndroidJavaObject>("getCurrentPosition");
				_position = new Position((float)ajo.Call<double>("getLatx"), (float)ajo.Call<double>("getLngx"));

								//ajo = androidGpsTracker.Call<AndroidJavaObject>("getPredictedPosition");
								//_predictedPosition = new Position((float)ajo.Call<double>("getLatx"), (float)ajo.Call<double>("getLngx"));
			}
		} catch (Exception e) {
			log.warning("Error getting position: " + e.Message);
//			errorLog = errorLog + "\ngetCurrentPosition|Bearing" + e.Message;
		}

		try {
			if (androidGpsTracker.Call<bool>("hasBearing")) {
				_bearing = androidGpsTracker.Call<float>("getCurrentBearing");
				//log.info("poll bearing");
			} else {
				_bearing = -999.0f;
			}
		} catch (Exception e) {
			log.warning("Error getting bearing: " + e.Message);
		}

	}

	public override void NotifyAutoBearing() {
 		try {
 			helper.Call("notifyAutoBearing");
 		} catch (Exception e) {
 			log.error(e, "Error notifying auto-bearing");
 		}

 	}

}
#endif