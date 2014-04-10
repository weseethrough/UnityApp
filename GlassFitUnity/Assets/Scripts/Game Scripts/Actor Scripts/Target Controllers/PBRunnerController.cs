using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class PBRunnerController : TargetController {
	
	private Animator anim; 

	// Use this for initialization
	public override void Start () {
		base.Start();	
	}
	
	public override void OnEnable() {
		base.OnEnable();

		anim = GetComponent<Animator>();
		anim.SetFloat("Speed", realWorldMovementSpeed);
		if(realWorldMovementSpeed > 2.2f && realWorldMovementSpeed < 4.0f) {
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
		if(realWorldMovementSpeed > 2.2f && realWorldMovementSpeed < 4.0f) {
			anim.speed = realWorldMovementSpeed / 2.2f;
		} else if(realWorldMovementSpeed > 4.0f) {
			anim.speed = Mathf.Clamp(realWorldMovementSpeed / 4.0f, 1, 2);
		} else if(realWorldMovementSpeed > 0.0f) {
			anim.speed = realWorldMovementSpeed / 1.25f;
		} else {
			anim.speed = 1.0f;
		}
	}
}
