using System;

#if UNITY_ANDROID
/// <summary>
/// Android platform. Overrides platform functionality with android-specific functionality where necessary. Usually this means JNI calls to the GlassfitPlatform libr
/// </summary>
public class AndroidPlatform : Platform
{

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


}
#endif

