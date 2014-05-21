using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;

using RaceYourself.Models;
using PositionTracker;

public class CrossPlatformPositionProvider : IPositionProvider {
	
	// List of listeners
	private List<IPositionListener> positionListeners = new List<IPositionListener>();

	private Log log = new Log("CrossPlatformPositionProvider");

	private float timeSinceLastUpdate = 0;

	private Position lastPosition;

	// Returns true in case of successful registration, false otherwise
	public bool RegisterPositionListener(IPositionListener posListener) {
		
		if (!Input.location.isEnabledByUser) {
			log.warning("Location not enabled by user");
			return false;
		}

		if (Input.location.status == LocationServiceStatus.Failed) {
			log.warning("Unable to determine device location");
			return false;
		}

		// If there is already listener - just push new one to the list
		if (positionListeners.Count > 0) {
			positionListeners.Insert(0, posListener);
			return true;
		}

		// Keep listener 
		positionListeners.Insert(0, posListener);
		
		// Update every second
		//InvokeRepeating("UpdateLocation", 0, 1);
		return true;

		//We can't use yield return outside of coroutines, and coroutines can't return a value (only IEnumerator).
		//Moreover, we wouldn't want to block execution for 20s while we wait for the registration.
		//I've moved Input.location.Start() to Platform.Initialise(), which should hopefully allow it to complete prior to any attempts to register a position listener.
		//That should work ok now, but we may want to defer it until it's actually needed (or check that it's definitely completed before doing any registering), in which case this might need restructuring to be done asynchronously.


        //Input.location.Start();
        //int maxWait = 20;

//        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
//            yield return new WaitForSeconds(1);
//            maxWait--;
//        }
//        if (maxWait < 1) {
//            //print("Timed out");
//			return false;
//        }

	}

	public void UnregisterPositionListener(IPositionListener posListener) {
		positionListeners.Remove(posListener);
		// If no listeners registered - stop update
		if (positionListeners.Count == 0) {
			Reset();
		}
	}

	public void Update() {

		LocationServiceStatus status = Input.location.status;
		if(status == LocationServiceStatus.Stopped)
		{
			UnityEngine.Debug.LogError("Location Service stopped");
		}
		if(status == LocationServiceStatus.Failed)
		{
			UnityEngine.Debug.LogError("Location Service failed");
		}

		if(status == LocationServiceStatus.Initializing)
		{
			UnityEngine.Debug.LogError("Location Service still initialising");
		}
		else
		{
			// UnityEngine.Debug.LogError("Location Service Running, timeSinceLastUpdate: " + timeSinceLastUpdate);
			//location service status should be 'Running'
			if(timeSinceLastUpdate > 1f)
			{
				UpdateLocation();
				timeSinceLastUpdate = 0;
			}
		}

		// increment timer
		timeSinceLastUpdate += Time.deltaTime;
	}

	//Renamed this to avoid clash with MonoBehaviour's own once-per-frame Update.
	public void UpdateLocation() {
		if (!Input.location.isEnabledByUser) {
			Reset();
			return;
		}

		if (Input.location.lastData.latitude == 0f && Input.location.lastData.longitude == 0f) {
			// invalid position, just return
			return;
		}

		// Create and fill position object
		Position pos = new Position(Input.location.lastData.latitude, Input.location.lastData.longitude);
		pos.device_ts = PositionTracker.Utils.CurrentTimeMillis ();
		pos.gps_ts = (long)(Input.location.lastData.timestamp*1000); // convert to milliseconds
		pos.epe = Input.location.lastData.horizontalAccuracy;
		pos.alt = Input.location.lastData.altitude;
		// TODO: take speed & bearing from native provider
		// Temporary calculate speed according to distance/time between 2 last positions
		if (lastPosition != null) {
			pos.speed = (float)PositionUtils.distanceBetween (pos, lastPosition) / ((pos.gps_ts - lastPosition.gps_ts) / 1000); 
		}

		UnityEngine.Debug.Log("New location: " + pos.latitude + " " + pos.longitude + ", device_ts: " + pos.device_ts + ", speed: " + pos.speed);

		//drop it on the HUD for debugging
		//DataVault.Set("sweat_points_unit", pos.latitude);
		//DataVault.Set("fps", pos.longitude);

		if(positionListeners.Count == 0)
		{
			UnityEngine.Debug.LogWarning("Location Updated, but no listeners!");
		}

		// Notify listeners about new position
		foreach(IPositionListener posListener in positionListeners) {
			posListener.OnPositionUpdate(pos);
			//UnityEngine.Debug.LogWarning("Location Update sent to a listener: " + posListener);
		}
		lastPosition = pos;
	}
	
	private void Reset() {
		positionListeners.Clear();
		//CancelInvoke("UpdateLocation");
		Input.location.Stop();		
	}
}
