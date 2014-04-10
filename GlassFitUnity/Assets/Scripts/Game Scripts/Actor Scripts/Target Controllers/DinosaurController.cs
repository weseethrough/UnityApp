using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the position of the Dinosaur and sounds.
/// </summary>
public class DinosaurController : TargetController {
	
	// Time until next scream
	private float screamTime = 0.0f;
	
	// Animator for the dinosaur
	private Animator anim;
	
	// Boolean to check if sound is playing
	private bool isPlaying = false;
	
	// Sound for the dinosaur scream
	private AudioSource scream;
	
	// Boolean to reset animation
	private bool isScream = false;
	
	// Original head rotation
	private Quaternion headRotation;
	
	// Starting distance
	private float distanceFromStart = -50f;
	
	// Players current distance
	private double playerDistance = 0.0;
	
	// Player's starting distance
	private double playerStartDistance = 0.0;
	
	// Time passed for speed up
	private float currentTime = 0;
	
	// Threshold for speed increase
	private float updateTime = 10f;
	
	// Value to increase speed by
	private float speedIncrease = 0.5f;
	
	// Current speed
	private float currentSpeed = 2.5f;
	
	/// <summary>
	/// Start this instance. Sets the initial attributes
	/// </summary>
	public override void Start () {
		
		// Start the base and set the attributes
		base.Start();

		// Get the animator
		anim = GetComponentInChildren<Animator>();
				
		// Get the scream sound
		scream = GetComponentInChildren<AudioSource>();
		
		
//		// Get the camera
//		cam = GameObject.Find("ARCamera");
//		UnityEngine.Debug.Log("Dino: Camera found");
	}
	
	/// <summary>
	/// Raises the enable event. Sets the attributes
	/// </summary>
	public void OnEnable() {
		// Enable the base and set the attributes.
		Reset();
		//SetAttribs(50, 135, -240, 0f);
	}
	
	/// <summary>
	/// Reset the attributes.
	/// </summary>
	public void Reset()
	{
		// Set the starting speed
		currentSpeed = 2.4f;
		// Set the start time
		currentTime = 0.0f;
		// Set the player's initial distance
		playerDistance = Platform.Instance.LocalPlayerPosition.Distance;
		playerStartDistance = Platform.Instance.LocalPlayerPosition.Distance;
		// Set the boulder's starting distance
		distanceFromStart = (float)playerDistance - 25f;
	}
	
	/// <summary>
	/// Update this instance. Updates the position and plays the sound if it is time
	/// </summary>
	public override void Update () {
		
		// If it screamed previously, make the animator bool false
		if(isScream)
		{
			if(anim != null) {
				anim.SetBool("Shout", false);
			}
			isScream = false;
			isPlaying = false;
		}
		
		// Update scream time
		screamTime += Time.deltaTime;
		
		if(screamTime > 9.5f && !isPlaying) 
		{
			scream.Play();	
			isPlaying = true;
		}
		
		// Set the scream boolean to true and play the sound
		if(screamTime > 15.0f) {
			//scream.Play();
			if(anim != null) {
				anim.SetBool("Shout", true);
			}
			screamTime -= 15.0f;
			isScream = true;
		}
		
		// Set the player distance 
		playerDistance = Platform.Instance.LocalPlayerPosition.Distance;
		
		// Increase the time
		currentTime += Time.deltaTime;
		
		// Update the speed if enough time has passed
		if(currentTime > updateTime) 
		{
			currentTime -= updateTime;
			
			currentSpeed += speedIncrease;
			
		}
		
		distanceFromStart += Time.deltaTime * currentSpeed;	
		
		// Update the base.
		base.Update();
	}
	
	/// <summary>
	/// Override base implementation, which queries target tracker.
	/// </summary>
	/// <returns>
	/// The distance behind this target.
	/// </returns>
	public override double GetDistanceBehindTarget ()
	{
		float relativeDist = distanceFromStart - (float)playerDistance;
		return relativeDist;
	}
	
	/// <summary>
	/// Gets the distance the player has travelled in the minigame.
	/// </summary>
	/// <returns>
	/// The player's distance.
	/// </returns>
	public double GetPlayerDistanceTravelled()
	{
		return playerDistance - playerStartDistance;
	}
}
