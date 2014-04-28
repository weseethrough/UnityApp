using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using RaceYourself.Models;
using PositionTracker;

public class CrossPlatformPositionProvider : MonoBehavior, IPositionProvider {
	
	// List of listeners
	private List<IPositionListener> positionListeners = new List<IPositionListener>();
	
	// Returns true in case of successful registration, false otherwise
	bool RegisterPositionListener(IPositionListener posListener) {
		
		if (!Input.location.isEnabledByUser)
            return false;
		
        // If there is already listener - just push new one to the list
		if (positionListeners.Count > 0) {
			positionListeners.Insert(0, posListener);
			return true;
		}
		
        Input.location.Start();
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        if (maxWait < 1) {
            //print("Timed out");
            return false;
        }
        if (Input.location.status == LocationServiceStatus.Failed) {
            //print("Unable to determine device location");
            return false;
        }
		// Keep listener 
		positionListeners.Insert(0, posListener);

		// Update every second
		InvokeRepeating("Update", 0, 1);
		return true;
	}
		
	void UnregisterPositionListener(IPositionListener posListener) {
		positionListeners.Remove(posListener);
		// If no listeners registered - stop update
		if (positionListeners.Count == 0) {
			Reset();
		}
	}
	
	public void Update() {
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
		CancelInvoke("Update");
		Input.location.Stop();		
	}
}
