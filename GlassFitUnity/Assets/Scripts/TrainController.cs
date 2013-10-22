using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class TrainController : MonoBehaviour {
	
	private float whistleTime = 0.0f;
	private double scaledDistance;
	private Platform inputData = null;
	private AudioSource trainMove;
	private AudioSource trainWhistle;
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
		
		var aSources = GetComponents<AudioSource>();
		trainMove = aSources[0];
		trainWhistle = aSources[1];
		trainMove.Play();
	}
	
	void OnEnable() {
		transform.position = new Vector3(103.8f, -300, -50);
	}
	
	// Update is called once per frame
	void Update () {
		
		whistleTime += Time.deltaTime;
		
		if(whistleTime >= 10.0f)
		{
			trainWhistle.Play();
			whistleTime -= 10.0f;
		}
		
		inputData.Poll();
	
		scaledDistance = (inputData.DistanceBehindTarget() - 50) * 135;

		Vector3 movement = new Vector3(103.8f,-300,(float)scaledDistance);
		transform.position = movement;
	}
}
