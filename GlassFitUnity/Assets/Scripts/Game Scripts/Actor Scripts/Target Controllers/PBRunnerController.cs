using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class PBRunnerController : TargetController {
	
	private Animator anim; 
	private float speed;
	
	// Use this for initialization
	public override void Start () {
		base.Start();	
		SetAttribs( 0, 3.0f, 0, 0.75f);
	}
	
	public override void OnEnable() {
		base.OnEnable();
		base.SetAttribs(0, 1, transform.position.y, transform.position.x);

		anim = GetComponent<Animator>();
		anim.SetFloat("Speed", realWorldMovementSpeed);
		if(speed > 2.2f && realWorldMovementSpeed < 4.0f) {
			anim.speed = realWorldMovementSpeed / 2.2f;
		} else if(realWorldMovementSpeed > 4.0f) {
			anim.speed = Mathf.Clamp(realWorldMovementSpeed / 4.0f, 1, 2);
		} else {
			anim.speed = realWorldMovementSpeed / 1.0f;
		}
	}
	
	public override void Update () {	
		//UnityEngine.Debug.Log("PBRunnerController: This is a PB Runner");
		base.Update();

		anim.SetFloat("Speed", realWorldMovementSpeed);
		if(speed > 2.2f && speed < 4.0f) {
			anim.speed = realWorldMovementSpeed / 2.2f;
		} else if(speed > 4.0f) {
			anim.speed = Mathf.Clamp(realWorldMovementSpeed / 4.0f, 1, 2);
		} else if(speed > 0.0f) {
			anim.speed = realWorldMovementSpeed / 1.25f;
		} else {
			anim.speed = 1.0f;
		}
	}
}
