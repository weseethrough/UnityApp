using System;
using RaceYourself.Models;
using UnityEngine;

#if UNITY_ANDROID
/// <summary>
/// Android platform. Overrides platform functionality with android-specific functionality where necessary. Usually this means JNI calls to the GlassfitPlatform libr
/// </summary>
public class AndroidPlatform : Platform
{
	private readonly AndroidJavaClass build_class = new AndroidJavaClass("android.os.Build");	

	private PlayerPosition _localPlayerPosition;
    public override PlayerPosition LocalPlayerPosition {
        get { return _localPlayerPosition; }
    }

	// Helper class for accessing/awarding points
	private PlayerPoints _playerPoints;
	public override PlayerPoints PlayerPoints { get { return _playerPoints; } }

	protected override void Initialize() {
		base.Initialize();
		log.info("Initializing AndroidPlayerPosition");
		_localPlayerPosition = new AndroidPlayerPosition();
		log.info("Initializing AndroidPlayerPoints");
		_playerPoints = new AndroidPlayerPoints();
	}
	
	
	public override Device DeviceInformation() 
	{
		return new Device(build_class.Get<string>("MANUFACTURER"), build_class.Get<string>("MODEL"));
	}	
}
#endif

