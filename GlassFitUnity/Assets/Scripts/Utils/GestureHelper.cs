using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

using SimpleJSON;

public class GestureHelper : MonoBehaviour {
	
	public bool isTapped = false;
	public bool left = false;
	public bool right = false;
	public bool up = false;
	public bool down = false;
	private float tapTimer = 0.0f;
	private float upTimer = 0.0f;
	private float downTimer = 0.0f;
	private float leftTimer = 0.0f;
	private float rightTimer = 0.0f;
	
	// Use this for initialization
	void Start () {
	
	}
	
	void isTap(string message) {
		isTapped = true;
		tapTimer = 5.0f;
		UnityEngine.Debug.Log("Message Obtained: Tap");
	}
	
	void flingLeft(string message) {
		left = true;
		leftTimer = 5.0f;
		UnityEngine.Debug.Log("Message Obtained: Fling Left");
	}
	
	void flingRight(string message) {
		right = true;
		rightTimer = 5.0f;
		UnityEngine.Debug.Log("Message Obtained: Fling Right");
	}
	
	void flingUp(string message) {
		up = true;
		upTimer = 5.0f;
	}
	
	void flingDown(string message) {
		down = true;
		downTimer = 5.0f;
	}
	
	// Update is called once per frame
	void Update () {
		
		tapTimer -= Time.deltaTime;
		if(tapTimer <= 0.0f) {
			isTapped = false;
		}
		
		leftTimer -= Time.deltaTime;
		if(leftTimer <= 0.0f) {
			left = false;
		}
		
		rightTimer -= Time.deltaTime;
		if(rightTimer <= 0.0f) {
			right = false;
		}
		
		upTimer -= Time.deltaTime;
		if(upTimer <= 0.0f) {
			up = false;
		}
		
		downTimer -= Time.deltaTime;
		if(downTimer <= 0.0f) {
			down = false;
		}
	}
}
