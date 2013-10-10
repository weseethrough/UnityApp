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
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
		
		scale.x = (float)Screen.width / originalWidth;
		scale.y = (float)Screen.height / originalHeight;
    	scale.z = 1;

	}
	
	void OnEnable() {
		transform.position = new Vector3(xDist, yDist, (float)scaledDistance);
	}
	
	void Update () {
				
		inputData.Poll();
		
		scaledDistance = inputData.DistanceBehindTarget() * 135;
		Vector3 movement = new Vector3(xDist, yDist,(float)scaledDistance);
		transform.position = movement;
	}
}
