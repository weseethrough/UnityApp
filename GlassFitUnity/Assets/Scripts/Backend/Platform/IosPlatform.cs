using System;
using RaceYourself.Models;
using System.Runtime.InteropServices;

using System.Collections.Generic;

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

    // iOS implementation of services
    private PlayerPoints _playerPoints = new LocalDbPlayerPoints ();
    public override PlayerPoints PlayerPoints { get { return _playerPoints; } }
    private PlayerPosition _localPlayerPosition = new IosPlayerPosition ();
    public override PlayerPosition LocalPlayerPosition { get { return _localPlayerPosition; } }
    private BleController _bleController;
    public override BleController BleController { get { return _bleController; } }

	Log log = new Log("IosPlatform");

	/// <summary>
	/// Initialize this instance.
	/// </summary>
	protected override void Initialize()
	{
		base.Initialize();

		//find and store the gesture helper object, to send messages to
		gh = (GestureHelper)Component.FindObjectOfType(typeof(GestureHelper));

		initialised = true;
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
        base.Update ();
        // TODO: pass iOS sensor orientation quaternion into base.playerPosition.Update(Quaternion q);
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

    public override Device DeviceInformation ()
    {
        return new Device ("Apple", "iUnknown");
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
		return true;
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
		//just use the Unity version
		return Input.touchCount;
    }

    // Returns (x,y) as floats between 0 and 1
    public override Vector2? GetTouchInput ()
    {
		if(GetTouchCount()>0)
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

	public override bool RequiresSoftwareBackButton()
	{
		return true;
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

