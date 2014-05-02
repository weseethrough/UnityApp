using System;
using RaceYourself.Models;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using AOT;
#if UNITY_IPHONE

using UnityEngine;


/// <summary>
/// Ios platform. Overrides platform functionality with iOS-specific functionality where necessary. Usually this means native iOS calls.
/// </summary>
public class IosPlatform : Platform
{

	//temp stuff for fling detection on iOS
	private Dictionary<int,TouchInfo> touches = new Dictionary<int, TouchInfo>();
	private GestureHelper gh = null;
	const float SWIPE_MIN_DIST = 10.0f;
	const float TAP_MAX_DIST = 2.0f;

	private string blobstore = "game-blob";
	private string blobassets = "blob";

    // iOS implementation of services
    private PlayerPoints _playerPoints = new LocalDbPlayerPoints ();
    public override PlayerPoints PlayerPoints { get { return _playerPoints; } }
    private PlayerPosition _localPlayerPosition = new IosPlayerPosition ();
    public override PlayerPosition LocalPlayerPosition { get { return _localPlayerPosition; } }
    private BleController _bleController;
    public override BleController BleController { get { return _bleController; } }
	private bool sentDeviceToken = false;

	Log log = new Log("IosPlatform");

	/// <summary>
	/// Initialize this instance.
	/// </summary>
	protected override void Initialize ()
	{
		UnityEngine.Debug.Log("Creating iOS Platform instance");
		
		blobstore = Path.Combine(Application.persistentDataPath, blobstore);
		blobassets = Path.Combine(Application.streamingAssetsPath, blobassets);
		
		var tag = "Player";
		if (!Application.isPlaying) {
			// Save to blob assets in editor
			blobstore = blobassets;
			tag = "Editor";
		}

		//report update frequency for gyros. May need to set it here too.
		float rate = Input.gyro.updateInterval;
		log.info("Gyro update interval: " + rate);

		//start notification services
		NotificationServices.RegisterForRemoteNotificationTypes(RemoteNotificationType.Alert | RemoteNotificationType.Badge | RemoteNotificationType.Sound);

		Directory.CreateDirectory(blobstore);
		UnityEngine.Debug.Log(tag + " blobstore: " + blobstore);
		if (Application.isEditor) Directory.CreateDirectory(blobassets);
		UnityEngine.Debug.Log(tag + " blobassets: " + blobassets);

		base.Initialize ();

		//find and store the gesture helper object, to send messages to
		gh = (GestureHelper)Component.FindObjectOfType(typeof(GestureHelper));

		initialised = true;

		//clear notification list notifications
		// TODO when we have an area to view challenges, don't clear this until they have been viewed
		NotificationServices.ClearRemoteNotifications();

	}

	/// <summary>
	/// Method which exists just to facilitate ahead of time compilation.
	/// </summary> 	
	private static void iOSUnusedMethod(){
		RaceYourself.FacebookMe fbme = new RaceYourself.FacebookMe();
		string s = fbme.id;
		string p = fbme.Picture;
	}

    /// <summary>
    /// Called every frame by PlatformPartner to update internal state
    /// </summary>
	[DllImport("__Internal")]
	private static extern void _Update();

	public override void Update ()
    {
		updateFlingDetection();

		//log.info("IosPlatform.Update");
		try {
			_Update();
		} catch(Exception e) {
			UnityEngine.Debug.LogException(e);
		}
        base.Update();


		//check for device token
		if(!sentDeviceToken)
		{
			byte[] token;
			if(NotificationServices.deviceToken != null)
			{
				String deviceTokenString = System.BitConverter.ToString(NotificationServices.deviceToken).Replace("-","");
				NetworkMessageListener.OnPushId(deviceTokenString);
				sentDeviceToken = true;
				log.info("Device token sent to network message listener: " + deviceTokenString);
			}
			else
			{
				if(NotificationServices.registrationError != null)
				{
					log.info("Error registering for notification services: " + NotificationServices.registrationError);
					sentDeviceToken = true;
				}
			}
		}

		//check for new notifications
		if(NotificationServices.remoteNotificationCount > 0)
		{
			log.info("New remote push notification received.");
			//clear it straight away for now.
			// TODO raise some event in non-platform code (NetworkMessageListener?) which will inform that there's a new notification, 
			// trigger a sync to get the full details, and do whatever else is necessary.
			NotificationServices.ClearRemoteNotifications();
		}

    }

	[DllImport("__Internal")]
	private static extern void _Poll();

    /// <summary>
    /// Called every frame *during* a race by RaceGame to update position, speed etc
    /// Not called outside races to save battery life
    /// </summary>
	public override void Poll ()
    {
		//log.info("IosPlatform.Poll");
		try { _Poll(); }
		catch(Exception e) {
			log.exception(e);
		}
        base.Poll ();
        // TODO: update any internal state that only needs to change *during* a race
    }

	[DllImport("__Internal")]
	private static extern string _getDeviceInfo();

	public override Device DeviceInformation ()
	{
		string deviceModel = _getDeviceInfo();
		log.info("device model : " + deviceModel);
		return new Device ("Apple", deviceModel);
	}


    public override bool OnGlass ()
    {
        return false;
    }

    public override bool IsRemoteDisplay ()
    {
        return false;
    }

	[DllImport("__Internal")]
	public static extern bool _IsPluggedIn();

    public override bool IsPluggedIn ()
    {
		log.info("IosPlatform.IsPluggedIn");
		bool result = false;
		try { _Poll(); }
		catch(Exception e) {
			log.exception(e);
		}
		return _IsPluggedIn();
    }

    public override bool HasInternet ()
    {
		//log.error("Not yet implemented for iOS");
		//TODO FIX
		return false;
    }

    public override bool HasWifi ()
    {
		//log.error("Not yet implemented for iOS");
		//TODO FIX
        //throw new NotImplementedException ();
		return false;
    }

    public override bool IsDisplayRemote ()
    {
		//log.error("Not yet implemented for iOS");
        //throw new NotImplementedException ();
		//TODO FIX
		return false;
    }

    public override bool HasGpsProvider ()
    {
		//log.error("Not yet implemented for iOS");
        //throw new NotImplementedException ();
		//TODO FIX
		return Input.location.status == LocationServiceStatus.Running;
    }
    // *** iOS implementation of bluetooth ***
    public override bool IsBluetoothBonded ()
    {
		//log.error("Not yet implemented for iOS");
        //throw new NotImplementedException ();
		//TODO FIX
		return false;
    }

    public override void BluetoothServer ()
    {
		//log.error("Not yet implemented for iOS");
        //throw new NotImplementedException ();
		//TODO FIX
		return;
    }

    public override void BluetoothClient ()
    {
		//log.error("Not yet implemented for iOS");
        //throw new NotImplementedException ();
		//TODO FIX
		return;
    }

    public override void BluetoothBroadcast (string json)
    {
		//log.error("Not yet implemented for iOS");
        //throw new NotImplementedException ();
		//TODO FIX
		return;
    }

    public override string[] BluetoothPeers ()
    {
		//log.error("Not yet implemented for iOS");
        //throw new NotImplementedException ();
		//TODO FIX
		return new string[0];
	}

	public override byte[] LoadBlob(string id) {
		try {
			UnityEngine.Debug.Log("PlatformDummy: Loading blob id: " + id);			
			return File.ReadAllBytes(Path.Combine(blobstore, id));			
		} catch (FileNotFoundException e) {
			return LoadDefaultBlob(id);
		}
	}
	
	public byte[] LoadDefaultBlob(string id) {
		try {
			UnityEngine.Debug.Log("PlatformDummy: Loading default blob id: " + id);
			if (blobassets.Contains("://")) {
				var www = new WWW(Path.Combine(blobassets, id));
				while(!www.isDone) {}; // block until finished
				return www.bytes;
			} else {
				return File.ReadAllBytes(Path.Combine(blobassets, id));			
			}
		} catch (FileNotFoundException e) {
			return new byte[0];
		}
	}
	
	public override void StoreBlob(string id, byte[] blob)
	{
		File.WriteAllBytes(Path.Combine(blobstore, id), blob);
		UnityEngine.Debug.Log("PlatformDummy: Stored blob id: " + id);
	}
	
	public override void ResetBlobs ()
	{
		//Not entirely sure what this is supposed to do. Wil do nothing for now. AH
		return;
	}
	
	public override void EraseBlob (String id)
	{
		throw new NotImplementedException ();
	}


	// *** iOS implementation of touch-input ***
    // May not need native (unity has some functions) - check before implementing
    // Returns the int number of fingers touching glass's trackpad
    public override int GetTouchCount ()
    {
		return Input.touchCount;
    }

    // Returns (x,y) as floats between 0 and 1
    public override Vector2? GetTouchInput ()
    {
		if(Input.touchCount>0)
		{
			Touch t = Input.GetTouch(0);
			return t.position;
		}
		else return null;
    }

    // *** iOS implementation of yaw ***
    // Should probably move to PlayerOrientation class
    public override float Yaw ()
    {
		//log.error("Not yet implemented for iOS");
        //throw new NotImplementedException ();
		return 0;
    }

    public override bool ProvidesBackButton()
	{
		return false;
	}

	protected void updateFlingDetection()
	{
			int tapCount = 0;
			
			for(int i=0; i<Input.touchCount; i++)
			{
				Touch touch = Input.touches[i];
				//collect touches beginning
				if(touch.phase == TouchPhase.Began)
				{
					TouchInfo ti = new TouchInfo(touch);
					touches.Add(touch.fingerId, ti);
				}
				
				//track touches moving
				if(touch.phase == TouchPhase.Moved)
				{
					if(touches.ContainsKey(touch.fingerId))
					{
						TouchInfo ti = touches[touch.fingerId];
						ti.distanceTravelled += touch.deltaPosition;
						ti.time += touch.deltaTime;
						
						///Removing these events for now. They shouldn't be required any longer
						// if they move far enough, count as a swipe
						if(ti.distanceTravelled.x <= -SWIPE_MIN_DIST)
						{
							//	swiped left
							UnityEngine.Debug.Log("ios platform: Touchscreen swipe left");
							touches.Remove(touch.fingerId);
							gh.SendMessage("FlingLeft", "iosPlatform");

						}
						else if (ti.distanceTravelled.x >= SWIPE_MIN_DIST)
						{
							//	swiped right
							UnityEngine.Debug.Log("ios platform: Touchscreen swipe right");
							touches.Remove(touch.fingerId);
							gh.SendMessage("FlingRight", "iosPlatform");
						}
					}
				}

				//track touches ending
				if(touch.phase == TouchPhase.Ended)
				{
					if(touches.ContainsKey(touch.fingerId))
					{
						TouchInfo ti = touches[touch.fingerId];
						
						// trigger tap if appropriate
						if (ti.distanceTravelled.magnitude <= TAP_MAX_DIST)
						{
							tapCount ++;
						}
						
						// remove from list
						touches.Remove(touch.fingerId);
					}
				}
				
				//track touches cancelled
				if(touch.phase == TouchPhase.Canceled)
				{					
					if(touches.ContainsKey(touch.fingerId))
					{
						touches.Remove(touch.fingerId);
					}
				}
			}	// /for all touches
	}
}
#endif

