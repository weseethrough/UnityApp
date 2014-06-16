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

		///NOTE having issues decoding the JSON coming from iOS. Data values aren't coming back as their original types. I suspect a problem with iOS's dictionary>json conversion
		/// My plan to work around this is to separately retrieve lastLat, lastLong, lastHeading, etc with individual calls to e.g.  double getLatestLat().
		/// It might be quite an inefficient way to do it since 


		//get the json position data back from iOS
		string json = _getLatestPosition();
		UnityEngine.Debug.Log("unity: json latest position: " + json);

		//deserialize
		Hashtable jsonObject = JsonConvert.DeserializeObject<Hashtable>(json);
		if(jsonObject == null)
		{
			UnityEngine.Debug.Log("Deserialise json position failed!");
		}
		//UnityEngine.Debug.Log(jsonObject.ToString());

		//construct a position
		UnityEngine.Debug.Log("getting data from json");

		double longitude = getDoubleFromHashTable( jsonObject, "longitude" );
		UnityEngine.Debug.Log("got longitude: " + longitude);

		double latitude = getDoubleFromHashTable(jsonObject, "latitude");
		UnityEngine.Debug.Log("got latitude: " + latitude);

		//magnetometer orientation of device
		double heading = getDoubleFromHashTable(jsonObject, "heading");
		UnityEngine.Debug.Log("got heading: " + heading);

		double epe = getDoubleFromHashTable(jsonObject, "epe");
		UnityEngine.Debug.Log("got epe: " + epe);

		double speed = getDoubleFromHashTable(jsonObject, "speed");
		UnityEngine.Debug.Log("got speed: " + speed);

		double ts = getDoubleFromHashTable(jsonObject, "ts");
		UnityEngine.Debug.Log("got ts: " + ts);

		Position pos = new Position((float)latitude, (float)longitude);
		pos.epe = (float)epe;
		pos.speed = (float)speed;
		pos.gps_ts = (long)ts;

		//direction of movement
		double course = getDoubleFromHashTable(jsonObject, "course");
		UnityEngine.Debug.Log("got course: " + course);
		pos.bearing = (float)course;

		return pos;
	}
	
	public static double getDoubleFromHashTable( Hashtable t, string key)
	{
		double result = 0.0;
		bool done = false;
		try {
			result = (double)t["key"];
			done = true;
		} 
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogWarning("couldn't extract double " + key + " from json");
		}

		if(!done)
		try {
			float fResult = (float)t["key"];
			result = (double)fResult;
			done = true;
		} catch (System.Exception e) {
			UnityEngine.Debug.LogWarning("couldn't extract float " + key + " from json. will try int");
		}

		//couldn't find double, try int
		if(!done)
		try {
			int iResult = (int)t["key"];
			result = (double)iResult;
			done = true;
		} catch (System.Exception e) {
			UnityEngine.Debug.LogWarning("couldn't extract int " + key + " from json. Will return 0.0");
		}

		return result;
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
			//return;
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
