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
		base.setAttribs(0, 135, -254.6f, 50);
		
		anim = GetComponent<Animator>();
		anim.speed = target.getCurrentSpeed() / 2.2f;
		speed = target.getCurrentSpeed();
		anim.SetFloat("Speed", speed);
	}
	
	void OnEnable() {
		base.OnEnable();
		base.setAttribs(0, 135, -254.6f, 50);
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
				anim.speed = newSpeed / 1.25f;
			}
		}
	}
}
