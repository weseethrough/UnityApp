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
	
	// Get the head of the dinosaur
	private GameObject dinoHead;
	
	// Get the camera
	private GameObject cam;
	
	/// <summary>
	/// Start this instance. Sets the initial attributes
	/// </summary>
	void Start () {
		
		// Start the base and set the attributes
		base.Start();
		SetAttribs(50, 135, -240, 0);
		
		// Get the animator
		anim = GetComponentInChildren<Animator>();
				
		// Get the scream sound
		scream = GetComponent<AudioSource>();
		
		// Get the dino's head
		dinoHead = GameObject.Find("UpperNeck");
		
		if(dinoHead==null)
		UnityEngine.Debug.Log("Dino: Upper neck not found");
		else
			UnityEngine.Debug.Log("Dino: Upper neck found");
		
//		// Get the camera
//		cam = GameObject.Find("ARCamera");
//		UnityEngine.Debug.Log("Dino: Camera found");
	}
	
	/// <summary>
	/// Raises the enable event. Sets the attributes
	/// </summary>
	void OnEnable() {
		// Enable the base and set the attributes.
		base.OnEnable();
		dinoHead = GameObject.Find("UpperNeck");
		
		if(dinoHead==null)
			UnityEngine.Debug.Log("Dino: Upper neck not found");
		else
			UnityEngine.Debug.Log("Dino: Upper neck found");
		
		SetAttribs(50, 135, -240, 0f);
	}
	
	/// <summary>
	/// Update this instance. Updates the position and plays the sound if it is time
	/// </summary>
	void Update () {
		
		// Update the base
		base.Update();
			
		// If it screamed previously, make the animator bool false
		if(isScream)
		{
			anim.SetBool("Shout", false);
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
		if(screamTime > 10.0f) {
			//scream.Play();
			anim.SetBool("Shout", true);
			screamTime -= 10.0f;
			isScream = true;
		}
	}
	
	/// <summary>
	/// Used to override the animation so the dinosaur looks at the player
	/// </summary>
//	void LateUpdate() {
//		if(dinoHead == null) 
//		{
//			dinoHead = GameObject.Find("UpperNeck");
//			UnityEngine.Debug.Log("Dino: head found");
//		}
//		
//		// Make the head look at the player
//		if(dinoHead != null)
//			dinoHead.transform.LookAt(Vector3.zero);
//		else
//			UnityEngine.Debug.Log("Dino: no head, no transform");
//	}
}
