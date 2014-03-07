using System;
using UnityEngine;

/// <summary>
/// Player points. All the logic currentlyimplemented in Java/Android. Should all be moved to Unity.
/// </summary>
public class AndroidPlayerPoints : PlayerPoints
{

	private AndroidJavaObject points_helper;
	private AndroidJavaClass points_helper_class;

	// Points earned since the last app restart or call to Reset()
	private long _currentActivityPoints = 0;
	public override long CurrentActivityPoints { get { return _currentActivityPoints; } }

	// Points earned over all time up to the last Reset() or app restart
	private long _openingPointsBalance = 0;
	public override long OpeningPointsBalance { get { return _openingPointsBalance; } }

	// Player's current gems
	public override int CurrentGemBalance { get {
		try {
			return points_helper.Call<int>("getCurrentGemBalance");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting current gem balance: " + e.Message);
			return 0;
		}
	}}

	// Player's current metabolism
	public override float CurrentMetabolism	{ get {
		try {
			return points_helper.Call<float>("getCurrentMetabolism");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting current metabolism: " + e.Message);
			return 0;
		}
	}}

	public AndroidPlayerPoints ()
	{
		log.profile("Connecting to Android points helper");

		try {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
	    	AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
	
			points_helper_class = new AndroidJavaClass("com.glassfitgames.glassfitplatform.points.PointsHelper");
			currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
				points_helper = points_helper_class.CallStatic<AndroidJavaObject>("getInstance", context);
			}));
		}
		catch (Exception e)
		{
			log.error("Error connecting to Android points helper: " + e.Message);
		}

		Update();

        UnityEngine.Debug.Log("Platform: Opening points: " + OpeningPointsBalance);
        UnityEngine.Debug.Log("Platform: Current game points: " + CurrentActivityPoints);
        UnityEngine.Debug.Log("Platform: Current gems: " + CurrentGemBalance);
        UnityEngine.Debug.Log("Platform: Current metabolism: " + CurrentMetabolism);

		log.profile("Connected to Android points helper");
	}

	public override void Update()
	{
		log.profile("Update() called");

		try {
			_currentActivityPoints = points_helper.Call<long>("getCurrentActivityPoints");
			UnityEngine.Debug.Log("Platform: poll current points");
			string pointsFormatted = _currentActivityPoints.ToString("n0");
			DataVault.Set ("points", pointsFormatted/* + "RP"*/);
			
			UnityEngine.Debug.Log("Platform: poll points vault set");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting current activity points: " + e.Message);
			DataVault.Set("points", -1);
		}		
		
		try {
			_openingPointsBalance = points_helper.Call<long>("getOpeningPointsBalance");
			UnityEngine.Debug.Log("Platform: poll opening points");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting opening points balance: " + e.Message);
		}

		log.profile("Update() complete");

	}

	public override void Reset() {
		try
		{
			points_helper.Call("reset");
			UnityEngine.Debug.Log("Platform: Playerpoints reset");
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error resetting playerpoints");
		}
	}
	
	public override void SetBasePointsSpeed(float speed) {
		try {
			points_helper.Call("setBaseSpeed", speed);
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error setting base points speed: " + e.Message);
		}
	}

	/// <summary>
	/// Use this method to award the user points.
	/// </summary>
	/// <param name='reason'>
	/// Reason that the points are being awarded, e.g. "1km bonus".
	/// </param>
	/// <param name='gameId'>
	/// Game identifier so we can log which game the points came from.
	/// </param>
	/// <param name='points'>
	/// Number of points to award.
	/// </param>
	public override void AwardPoints(String reason, String gameId, long points)
	{
		try
		{
			points_helper.Call("awardPoints", "in-game bonus", reason, gameId, points);
			UnityEngine.Debug.Log("Platform: " + gameId + " awarded " + points + " points for " + reason);
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error awarding " + reason + " of " + points + " points in " + gameId);
		}
	}
	
	/// <summary>
	/// Use this method to award the user gems.
	/// </summary>
	/// <param name='reason'>
	/// Reason that the gems are being awarded, e.g. "race completion".
	/// </param>
	/// <param name='gameId'>
	/// Game identifier so we can log which game the gems came from.
	/// </param>
	/// <param name='gems'>
	/// Number of gems to award.
	/// </param>
	public override void AwardGems(String reason, String gameId, int gems)
	{
		try
		{
			points_helper.Call("awardGems", "in-game bonus", reason, gameId, gems);
			UnityEngine.Debug.Log("Platform: " + gameId + " awarded " + gems + " gem(s) for " + reason);
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error awarding " + reason + " of " + gems + " gems in " + gameId);
		}
	}


}


