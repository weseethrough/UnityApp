using System;
using RaceYourself.Models;

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
    private PlayerPosition _localPlayerPosition = new CrossPlatformPlayerPosition ();
    public override PlayerPosition LocalPlayerPosition { get { return _localPlayerPosition; } }
    private BleController _bleController;
    public override BleController BleController { get { return _bleController; } }

    /// <summary>
    /// Called every frame by PlatformPartner to update internal state
    /// </summary>
    public override void Update ()
    {
        base.Update ();

		//TODO, if this is successful, move to base Platform
		Quaternion orientation = Input.gyro.attitude;
		playerOrientation.Update(orientation);

    }

	protected override void Initialize ()
	{
		//report update frequency for gyros. May need to set it here too.
		float rate = Input.gyro.updateInterval;
		log.info("Gyro update interval: " + rate);

		base.Initialize ();
	}

    /// <summary>
    /// Called every frame *during* a race by RaceGame to update position, speed etc
    /// Not called outside races to save battery life
    /// </summary>
    public override void Poll ()
    {
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

    public override bool IsPluggedIn ()
    {
        throw new NotImplementedException ();
    }

    public override bool HasInternet ()
    {
        throw new NotImplementedException ();
    }

    public override bool HasWifi ()
    {
        throw new NotImplementedException ();
    }

    public override bool IsDisplayRemote ()
    {
        throw new NotImplementedException ();
    }

    public override bool HasGpsProvider ()
    {
        throw new NotImplementedException ();
    }
    // *** iOS implementation of bluetooth ***
    public override bool IsBluetoothBonded ()
    {
        throw new NotImplementedException ();
    }

    public override void BluetoothServer ()
    {
        throw new NotImplementedException ();
    }

    public override void BluetoothClient ()
    {
        throw new NotImplementedException ();
    }

    public override void BluetoothBroadcast (string json)
    {
        throw new NotImplementedException ();
    }

    public override string[] BluetoothPeers ()
    {
        throw new NotImplementedException ();
    }
    // *** iOS implementation of blob-storage ***
    // Will likely be replaced by database soon - don't bother implementing
    public override byte[] LoadBlob (string id)
    {
        throw new NotImplementedException ();
    }

    public override void StoreBlob (string id, byte[] blob)
    {
        throw new NotImplementedException ();
    }

    public override void EraseBlob (string id)
    {
        throw new NotImplementedException ();
    }

    public override void ResetBlobs ()
    {
        throw new NotImplementedException ();
    }
    // *** iOS implementation of touch-input ***
    // May not need native (unity has some functions) - check before implementing
    // Returns the int number of fingers touching glass's trackpad
    public override int GetTouchCount ()
    {
        throw new NotImplementedException ();
    }
    // Returns (x,y) as floats between 0 and 1
    public override Vector2? GetTouchInput ()
    {
        throw new NotImplementedException ();
    }
    // *** iOS implementation of yaw ***
    // Should probably move to PlayerOrientation class
    public override float Yaw ()
    {
        throw new NotImplementedException ();
    }
}
#endif

