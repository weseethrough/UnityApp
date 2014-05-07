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

		//TODO - only once a second
		UpdateLocation();
	}

	//Renamed this to avoid clash with MonoBehaviour's own once-per-frame Update.
	public void UpdateLocation() {
		if (!Input.location.isEnabledByUser) {
			Reset();
			return;
		}
			
		Position pos = new Position(Input.location.lastData.latitude, Input.location.lastData.longitude);
		
		// Notify listeners about new position
		foreach(IPositionListener posListener in positionListeners) {
			posListener.OnPositionUpdate(pos);
		}
	}
	
	private void Reset() {
		positionListeners.Clear();
		//CancelInvoke("UpdateLocation");
		Input.location.Stop();		
	}
}
