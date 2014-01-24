using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class PBRunnerController : TargetController {
	
	private Animator anim; 
	private float speed;
	
	// Use this for initialization
	void Start () {
		base.Start();	
		SetAttribs( 0, 3.0f, 0, 1.5f);
	}
	
	void OnEnable() {
		base.OnEnable();
		base.SetAttribs(0, 1, transform.position.y, transform.position.z);
		
#if !UNITY_EDITOR
		anim = GetComponent<Animator>();
		if(target != null)
		{
			speed = target.PollCurrentSpeed();
			anim.SetFloat("Speed", speed);
			if(speed > 2.2f && speed < 4.0f) {
				anim.speed = speed / 2.2f;
			} else if(speed > 4.0f) {
				anim.speed = Mathf.Clamp(speed / 4.0f, 1, 2);
			} else {
				anim.speed = speed / 1.0f;
			}
		}
		else
		{
			UnityEngine.Debug.LogWarning("PBRunnerController: Target is null in OnEnable");
		}
#endif
	}
	
	void Update () {				
		base.Update();
#if !UNITY_EDITOR
		float newSpeed = target.PollCurrentSpeed();
		if(speed != newSpeed)
		{
			speed = newSpeed;
			anim.SetFloat("Speed", speed);
			if(speed > 2.2f && speed < 4.0f) {
				anim.speed = newSpeed / 2.2f;
			} else if(speed > 4.0f) {
				anim.speed = Mathf.Clamp(newSpeed / 4.0f, 1, 2);
			} else {
				anim.speed = newSpeed / 1.0f;
			}
		}
#endif
	}
}
