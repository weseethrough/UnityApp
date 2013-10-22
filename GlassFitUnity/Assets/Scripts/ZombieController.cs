using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class ZombieController : MonoBehaviour {

	private Platform inputData = null;
	private Animator anim;
	private float speed;
	
	private double scaledDistance;
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
	}
	
	void OnEnable() {
		transform.position = new Vector3(-10, -239.5f, 0);
	}
		
	void OnGUI() {
		
	}
	
	void Update () {

		inputData.Poll();
	
		scaledDistance = (inputData.DistanceBehindTarget()-20) * 135;
		Vector3 movement = new Vector3(-10, -239.5f,(float)scaledDistance);
		transform.position = movement;
	}
}
