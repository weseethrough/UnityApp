using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the music in the Music game
/// </summary>
public class SoundController : MonoBehaviour {
	
	// Number of audio tracks.
	private const float NUM_TRACKS = 11;
	
	// Array of audio sources for the tracks.
	private AudioSource[] stevies;
	
	// Current time to calculate when to start next track.
	private float curTime = 0.0f;
	
	// Text for indoor mode.
	private string indoorText = "Indoor Active";
	
	// Boolean for indoor/outdoor mode.
	private bool indoor = true;
	
	// Score based on player's performance.
	private float score = 0.0f;
	
	// Multiplier.
	private int mult = 1;
	
	// Height, width and scale factor.
	private int originalHeight = 500;
	private int originalWidth = 800;
	private Vector3 scale = Vector3.one;
	
	// Countdown and started variables.
	private bool countdown = false;
	private float countTime = 3.0f;
	private bool started = false;
	
	// Current track the player is on.
	private int currentTrack = 1;
	
	/// <summary>
	/// Initialises the music tracks
	/// </summary>
	void Start () {
		
		// Get all audio tracks.
		stevies = GetComponents<AudioSource>();
		
		// Set indoor mode and speed.
		Platform.Instance.SetIndoor(true);
		Platform.Instance.SetTargetSpeed(1.5f);
		
		// Set scale values.
		scale.x = (float)Screen.width/originalWidth;
		scale.y = (float)Screen.height/originalHeight;
	}
	
	/// <summary>
	/// Raises the GU event. Sets the buttons - needs updating
	/// </summary>
	void OnGUI() {
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		// Button for Indoor mode.
		// TODO: Update for nGUI.
		if(GUI.Button(new Rect(originalWidth/2-100, originalHeight-100, 200, 100), indoorText))
		{
			if(indoor)
			{
				indoor = false;
				indoorText = "Outdoor Active";
				Platform.Instance.StopTrack();
				Platform.Instance.Reset();
				Platform.Instance.SetIndoor(indoor);
				
				score = 0;
				mult = 1;
				curTime = 0.0f;
				countdown = false;
				started = false;
				countTime = 3.0f;
				currentTrack = 1;
				for(int i=1; i<NUM_TRACKS; i++)
				{
					stevies[i].volume = 0.0f;
				}
			}
			else
			{
				indoor = true;
				indoorText = "Indoor Active";
				Platform.Instance.StopTrack();
				Platform.Instance.Reset();
				Platform.Instance.SetIndoor(indoor);
				//Platform.Instance.StartTrack(indoor);
				score = 0;
				mult = 1;
				curTime = 0.0f;
				countdown = false;
				started = false;
				countTime = 3.0f;
				currentTrack = 1;
				for(int i=1; i<NUM_TRACKS; i++)
				{
					stevies[i].volume = 0.0f;
				}
			}
		}
		GUI.Label(new Rect(originalWidth/2-100, 0, 200, 100), "Current Score: " + score.ToString("f0"));
		
		if(countdown)
		{
			GUI.skin.label.fontSize = 40;
			int cur = Mathf.CeilToInt(countTime);
			if(countTime > 0.0f)
			{
				GUI.Label(new Rect(400, 200, 200, 200), cur.ToString()); 
			}
			else if(countTime > -1.0f && countTime < 0.0f)
			{
				GUI.Label(new Rect(400, 200, 200, 200), "GO!"); 
			}
		}
	}
	
	/// <summary>
	/// Update this instance. Updates the playing track
	/// </summary>
	void Update () {
		
		// Update platform values.
		Platform.Instance.Poll();
		
		// If indoor mode or gps has a lock, start countdown.
		if(Platform.Instance.HasLock() || indoor)
		{
			countdown = true;
		 	if(countTime <= -1.0f && !started)
			{
				Platform.Instance.StartTrack();
				UnityEngine.Debug.LogWarning("Tracking Started");
				started = true;
			}
			else if(countTime > -1.0f)
			{
				UnityEngine.Debug.LogWarning("Counting Down");
				countTime -= Time.deltaTime;
			}
		}
		
		// Get actual distance behind target based on offset.
		double dist = Platform.Instance.DistanceBehindTarget() - 50;
		
		// If player is ahead, increase the score and bring in more tracks.
		if(dist <= 0.0 && started)
		{
			// Increase time.
			curTime += Time.deltaTime;
			
			// If time is greater than threshold.
			if(curTime > 3.0f)
			{
				// reset time
				curTime -= 3.0f;
				
				// Update multiplier if in range.
				if(mult < 4)
				{
					mult++;
				}
				
				// If another track is available, bring it in.
				if(currentTrack < NUM_TRACKS)
				{
					stevies[currentTrack].volume = 1.0f;
					currentTrack++;
				}
			}
			
			// Increase the score.
			score += Time.deltaTime * mult;
		}
		// Else if target is ahead.
		else if(started && dist > 0.0)
		{
			// Reduce time for multipler.
			curTime -= Time.deltaTime;
			
			// If there is a multiplier, reset it.
			if(mult > 1)
			{
				mult = 1;
			}
			
			// If time reaches threshold and there are tracks left.
			if(curTime < 0.0f && currentTrack > 0)
			{
				// Reduce the volume of current track.
				stevies[currentTrack].volume = 0.0f;
				curTime += 3.0f;
				
				// Reduce the current track.
				currentTrack--;
			}
		}
		
		
		
	}
}
