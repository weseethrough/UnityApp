using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class PBRunnerController : MonoBehaviour {
	
	private Platform inputData = null;
	
	private float countTime = 3.99f;
	private bool started = false;
	
	private double scaledDistance;
	private float yDist = -254.6f;
	private float xDist = 50;
	
	private Vector3 scale;
	private int originalWidth = 800;
	private int originalHeight = 500;
	
	private Animator anim; 
	private float speed;
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
		anim = GetComponent<Animator>();
		scale.x = (float)Screen.width / originalWidth;
		scale.y = (float)Screen.height / originalHeight;
    	scale.z = 1;
		anim.speed = inputData.getCurrentSpeed(0) / 2.2f;
		speed = inputData.getCurrentSpeed(0);
	}
	
	void OnEnable() {
		transform.position = new Vector3(xDist, yDist, (float)scaledDistance);
	}
	
	void Update () {
				
		inputData.Poll();
		
		float newSpeed = inputData.getCurrentSpeed(0);
		if(speed != newSpeed)
		{
			speed = newSpeed;
			if(speed > 2.2f && speed < 4.0f) {
				anim.speed = newSpeed / 2.2f;
			} else if(speed > 4.0f) {
				anim.speed = newSpeed / 4.0f;
			} else {
				anim.speed = newSpeed / 1.25f;
			}
		}
		
		scaledDistance = inputData.DistanceBehindTarget() * 135;
		Vector3 movement = new Vector3(xDist, yDist,(float)scaledDistance);
		transform.position = movement;
	}
}
