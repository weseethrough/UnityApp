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
	}
	
	void OnEnable() {
		base.OnEnable();
		base.setAttribs(0, 135, -254.6f, 50);
		
		anim = GetComponent<Animator>();
		speed = target.getCurrentSpeed();
		anim.SetFloat("Speed", speed);
		if(speed > 2.2f && speed < 4.0f) {
			anim.speed = speed / 2.2f;
		} else if(speed > 4.0f) {
			anim.speed = Mathf.Clamp(speed / 4.0f, 1, 2);
		} else {
			anim.speed = speed / 1.0f;
		}
	}
	
	void Update () {
				
		base.Update();
		float newSpeed = target.getCurrentSpeed();
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
	}
}
