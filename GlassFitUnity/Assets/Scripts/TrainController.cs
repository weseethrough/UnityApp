using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class TrainController : MonoBehaviour {
	
	private float countTime = 3.0f;
	private float trainTime = 0f;
	private float whistleTime = 0.0f;
	private bool started = false;
	private double scaledDistance;
	private Platform inputData = null;
	private AudioSource trainMove;
	private AudioSource trainWhistle;
	public bool indoor = false;
	
	private Vector3 scale;
	private int originalHeight = 500;
	private int originalWidth = 800;

	public void reset()
	{
		UnityEngine.Debug.Log("Train: position reset");
		//inputData = new Platform();
		scaledDistance = -1000f;
	}
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
		
		var aSources = GetComponents<AudioSource>();
		trainMove = aSources[0];
		trainWhistle = aSources[1];
		
		scale.x = (float)Screen.width / originalWidth;
		scale.y = (float)Screen.height / originalHeight;
    	scale.z = 1;
		
		trainMove.Play();
	}
	
	void OnEnable() {
		transform.position = new Vector3(103.8f, -154f, -50);
		//inputData = new Platform();
		//Debug.Log("OnEnable called\n\n\n\n\n");
	}
	
	void OnGUI() {
				
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
	
		scaledDistance = (inputData.DistanceBehindTarget() - 50) * 76.666f;

		Vector3 movement = new Vector3(103.8f,-154.4f,(float)scaledDistance);
		transform.position = movement;
	}
	
	string SiDistance(double meters) {
		string postfix = "m";
		string final;
		float value = (float)meters;
		if (value > 1000) {
			value = value/1000;
			postfix = "km";
			final = value.ToString("f3");
		}
		else
		{
			final = value.ToString("f0");
		}
		return final+postfix;
	}
}
