using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using PositionTracker;
using RaceYourself.Models;

#if UNITY_IPHONE
public class IosPositionProvider : MonoBehaviour, IPositionProvider {

	int currentAuthorisationStatus = 0;

	float timeSinceLastPositionCheck = 0;
	bool gpsRunning = false;
	private List<IPositionListener> positionListeners = new List<IPositionListener>();

	/// Methods from iOS code

	[DllImport("__Internal")]
	private static extern void _startGPS();

	[DllImport("__Internal")]
	private static extern void _stopGPS();

	/// <summary>
	/// _gets the latest position.
	/// </summary>
	/// <returns>JSON dictionary with double:latitude, double:longitude, double:heading, double:course </returns>
	[DllImport("__Internal")]
	private static extern string _getLatestPosition();

	private static Position getLatestPosition()
	{
		//get the json position data back from iOS
		string json = _getLatestPosition();
		UnityEngine.Debug.Log("unity: json latest position: " + json);

		//deserialize
		Hashtable jsonObject = JsonConvert.DeserializeObject<Hashtable>(json);

		UnityEngine.Debug.Log(jsonObject.ToString());

		//construct a position
		double longitude = (double)jsonObject["longitude"];
		double latitude = (double)jsonObject["latitude"];

		//magnetometer orientation of device
		double heading = (double)jsonObject["heading"];

		//direction of movement
		double course = (double)jsonObject["course"];

		double epe = (double)jsonObject["epe"];
		double speed = (double)jsonObject["speed"];
		double ts = (double)jsonObject["ts"];

		Position pos = new Position((float)latitude, (float)longitude);
		pos.bearing = (float)course;
		pos.epe = (float)epe;
		pos.speed = (float)speed;
		pos.gps_ts = (long)ts;

		return pos;
	}

	public IosPositionProvider()
	{
		_startGPS();
	}

	/// <summary>
	/// gets the authorisation status. Value reflects apple's values:
	///    kCLAuthorizationStatusNotDetermined = 0,
	///    kCLAuthorizationStatusRestricted,
	///    kCLAuthorizationStatusDenied,
	///    kCLAuthorizationStatusAuthorized
	/// </summary>
	[DllImport("__Internal")]
	private static extern int _getAuthorisationStatus();

	//// Update is called once per frame
	public void Update () {
		
		//check status, and set a message if appropriate
		int newStatus = _getAuthorisationStatus();
		if(currentAuthorisationStatus != newStatus)
		{
			currentAuthorisationStatus = newStatus;
			switch(newStatus)
			{
			case 0:
				// not determined
				DataVault.Set("location_service_status_message", "Location service status undetermined");
				break;
			case 1:
				//Restricted - and user cannot enable. e.g. parental controls
				DataVault.Set("location_service_status_message", "Location service not available");
				break;
			case 2:
				//Denied - and user can enable
				UnityEngine.Debug.Log("iOS Location service disabled");
				DataVault.Set("location_service_status_message", "Please enable location services for Race Yourself in Settings");
				break;
			case 3:
				//Authorised
				DataVault.Set("location_service_status_message", "Location service enabled");
				break;
			}
		}

		if(!Platform.Instance.LocalPlayerPosition.IsTracking)
		{
			return;
		}
		//UnityEngine.Debug.Log("iOSPositionProvider: Update");

		timeSinceLastPositionCheck += Time.deltaTime;
		if(timeSinceLastPositionCheck > 1.0f)
		{
			UnityEngine.Debug.Log("iOSPositionProvide: Time to fetch new position");
			timeSinceLastPositionCheck = 0f;
			Position latestPos = getLatestPosition();
			foreach(IPositionListener listener in positionListeners)
			{
				listener.OnPositionUpdate(latestPos);
			}
		}
	}

	public bool RegisterPositionListener (IPositionListener posListener)
	{
		if(!gpsRunning)
		{
			_startGPS();
			gpsRunning = true;
		}
		positionListeners.Insert(0,posListener);
		return true;
	}
	
	public void UnregisterPositionListener (IPositionListener posListener)
	{
		positionListeners.Remove(posListener);
	}
}
#endif
