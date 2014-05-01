using System;
using RaceYourself.Models;
using System.Runtime.InteropServices;


#if UNITY_IPHONE
using UnityEngine;

/// <summary>
/// Ios platform. Overrides platform functionality with iOS-specific functionality where necessary. Usually this means native iOS calls.
/// </summary>
public class IosPlatform : Platform
{


    // iOS implementation of services
    private PlayerPoints _playerPoints = new LocalDbPlayerPoints ();
    public override PlayerPoints PlayerPoints { get { return _playerPoints; } }
    private PlayerPosition _localPlayerPosition = new IosPlayerPosition ();
    public override PlayerPosition LocalPlayerPosition { get { return _localPlayerPosition; } }

	private bool sentDeviceToken = false;

	Log log = new Log("IosPlatform");

	/// <summary>
	/// Initialize this instance.
	/// </summary>
	protected override void Initialize ()
	{
		//report update frequency for gyros. May need to set it here too.
		float rate = Input.gyro.updateInterval;
		log.info("Gyro update interval: " + rate);

		//start notification services
		NotificationServices.RegisterForRemoteNotificationTypes(RemoteNotificationType.Alert | RemoteNotificationType.Badge | RemoteNotificationType.Sound);


		base.Initialize ();
		initialised = true;
	}

    /// <summary>
    /// Called every frame by PlatformPartner to update internal state
    /// </summary>
	[DllImport("__Internal")]
	private static extern void _Update();

	public override void Update ()
    {
		//log.info("IosPlatform.Update");
		try {
			_Update();
		} catch(Exception e) {
			UnityEngine.Debug.LogException(e);
		}
        base.Update ();

		if(!sentDeviceToken)
		{
			byte[] token;
			if(NotificationServices.deviceToken != null)
			{
				String deviceTokenString = System.BitConverter.ToString(NotificationServices.deviceToken);
				_networkMessageListener.OnPushId(deviceTokenString);
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
		return new Device ("Apple", "iUnknown");
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

//	[DllImport("__Internal")]
//	private static extern byte[] _LoadBlob (string id);
//
//    // *** iOS implementation of blob-storage ***
//    // Will likely be replaced by database soon - don't bother implementing
//    public override byte[] LoadBlob (string id)
//    {
//		log.error("Load Blob Unity call: " + id);
//        //throw new NotImplementedException ();
//		return _LoadBlob (id);
//    }
//
//	[DllImport("__Internal")]
//	private static extern byte[] _StoreBlob (string id, byte[] blob);
//
//    public override void StoreBlob (string id, byte[] blob)
//    {
//		log.error("Store Blob Unity call: " + id);
//        //throw new NotImplementedException ();
//		_StoreBlob(id, blob);
//		return;
//    }

    public override void EraseBlob (string id)
    {
		log.error("Not yet implemented for iOS");
		//throw new NotImplementedException ();
		return;
    }

    public override void ResetBlobs ()
    {
		log.error("Not yet implemented for iOS");
        //throw new NotImplementedException ();
		return;
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
		return null;
    }
    // *** iOS implementation of yaw ***
    // Should probably move to PlayerOrientation class
    public override float Yaw ()
    {
		//log.error("Not yet implemented for iOS");
        //throw new NotImplementedException ();
		return 0;
    }

	public override bool RequiresSoftwareBackButton()
	{
		return true;
	}
}
#endif

