using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using RaceYourself;

// NOTE this class won't currently work. Refactor required to use position controller - AH

/// <summary>
/// Controls the eagle's movement and sound
/// </summary>
public class EagleController : WorldObject {

	// Animation speed.
	private float speed;
	
	// Screeching sound.
	private AudioSource screech;
	
	// Boolean for when the eagle is descending.
	private bool descend = false;
	
	// Animator and time passed for screeching.
	private Animator anim;
	float screechTime = 0.0f;
	
	/// <summary>
	/// Start this instance. Initialises attributes and animation
	/// </summary>
	public override void Start () {
		UnityEngine.Debug.LogError("Using deprecated class. Refactor required.");
		// Start the base and get the animator.
		base.Start();
		anim = GetComponent<Animator>();
		
		// Set the animation to fly.
		anim.SetBool("Attack", false);
	
		// Get the audiosource.
		screech = GetComponent<AudioSource>();
		//StartCoroutine(FlyingMovement(20));
	}
	
	/// <summary>
	/// Controls the movement
	/// </summary>
	public override void Update () {
		// Update the base.
		base.Update();
		
		// Find the distance behind target based on the offset
//#if !UNITY_EDITOR
//		float realDist = (float)target.GetDistanceBehindTarget() - (float)distanceOffset;
//#else
//        float realDist = (float)Platform.Instance.DistanceBehindTarget() - (float)distanceOffset;
//#endif
//		if(realDist < -49)
//		{
//			height = 2092;
//		}
//		
//		// If the eagle is within grabbing range.
//		if(realDist > -30 && realDist <= 0)
//		{
//			// If not already descending, set animation to attack and screech.
//			if(!descend)
//			{
//				anim.SetBool("Attack", true);
//				screech.Play();
//				screechTime = 0.0f;
//				descend = true;
//			}
//			
//			// Play sound if enough time has passed.
//			if(screechTime > 15.0f)
//			{
//				screechTime -= 15;
//				screech.Play();
//			}
//			
//			// Update screech time.
//			screechTime += Time.deltaTime;
//			
//			// Calculate the time it would take to reach the player.
//#if !UNITY_EDITOR
//			float time = -realDist / target.PollCurrentSpeed();
//#else
//            float time = -realDist / Platform.Instance.GetTargetSpeed();
//#endif
//			// Then calculate the speed of descent.
//			speed = height / time;
//			
//			// If the eagle isn't low enough, descend.
//			if(height > 0)
//			{
//				height -= speed * Time.deltaTime;
//			}
//		}
//		else
//		{
//			// Set animation to fly again and ascend if not high enough.
//			anim.SetBool("Attack", false);
//			descend = false;
//			speed = 500;
//			if(height < 2092)
//			{
//				height += speed * Time.deltaTime;
//			}
//		}
		
	}
	
	IEnumerator FlyingMovement(float time) {
		
		// Do movement
		
		
		yield return new WaitForSeconds(time);
		UnityEngine.Debug.Log("Eagle: Testing the descent");
		StartCoroutine(Descending());
	}
	
	IEnumerator Descending() {
		
		// Start descending
		
		
		yield return new WaitForSeconds(10);
		UnityEngine.Debug.Log("Eagle: Testing the Flying");
		StartCoroutine(FlyingMovement(20));
	}
}
