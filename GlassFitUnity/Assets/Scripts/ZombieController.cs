using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class ZombieController : MonoBehaviour {

	private Platform inputData = null;
	
	public GameObject platform;
	private Transform CurrentLocation;
	private Transform targetLocation;
	private float myDistance;
	private float targetDistance;
	public bool indoor = false;
	
	private float countTime = 3.99f;
	private bool started = false;
	
	private float scaledPace;
	private float paceSlider;
	private Rect sliderBox;
	private float indoorDistance;
	private float timeChange;
	private double scaledDistance;
	
	private Rect target;	
	private string targetText;
	
	private Vector3 scale;
	private int originalWidth = 800;
	private int originalHeight = 500;

	// Use this for initialization
	void Start () {
		inputData = new Platform();
		target =   new Rect(15, 15, 200, 100);
		
		scale.x = (float)Screen.width / originalWidth;
		scale.y = (float)Screen.height / originalHeight;
    	scale.z = 1;
	}
	
	void OnEnable() {
		transform.position = new Vector3(-10, -81.7f, 0);
		//inputData = new Platform(); 
	}
		
	void OnGUI() {
		
	}
	
	void Update () {

		inputData.Poll();
	
		scaledDistance = (inputData.DistanceBehindTarget()-20) * 76.666f;
		Vector3 movement = new Vector3(-10, -81.7f,(float)scaledDistance);
		transform.position = movement;
	}
	
	public void reset()
	{
		scaledDistance = 0;
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
