using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class EagleController : MonoBehaviour {

	private double scaledDistance;
	private Platform inputData = null;
	public bool indoor = false;
	
	private float speed;
	private AudioSource screech;
	private bool descend = false;
	
	private Animator anim;
	
	private Vector3 scale;
	private int originalHeight = 500;
	private int originalWidth = 800;
	private float height = 592.0f;
	
	public void reset()
	{
		UnityEngine.Debug.Log("Eagle: position reset");
		scaledDistance = -1000f;
	}
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
		
		scale.x = (float)Screen.width / originalWidth;
		scale.y = (float)Screen.height / originalHeight;
    	scale.z = 1;

		anim = GetComponent<Animator>();
		anim.SetBool("Attack", false);
	
		screech = GetComponent<AudioSource>();
		
	}
	
	void OnEnable() {
		transform.position = new Vector3(0, height, -50);
	}
	
	void OnGUI() {

	}
	
	// Update is called once per frame
	void Update () {
				
		inputData.Poll();
	
		float realDist = (float)inputData.DistanceBehindTarget() - 50;
		scaledDistance = realDist * 76.666f;

		Vector3 movement = new Vector3(0,height,(float)scaledDistance);
		transform.position = movement;
		
		//UnityEngine.Debug.Log("Eagle: Distance is :" + realDist.ToString());
		if(realDist > -30 && realDist <= 0)
		{
			if(!descend)
			{
				anim.SetBool("Attack", true);
				float time = -realDist / inputData.getCurrentSpeed(0);
				speed = height / time;
				screech.Play();
				descend = true;
			}
			
			if(height > 0)
			{
				height -= speed * Time.deltaTime;
				//UnityEngine.Debug.Log("Eagles height is: " + height.ToString());
			}
		}
		else
		{
			anim.SetBool("Attack", false);
			descend = false;
			//anim.animation["EagleFly"].wrapMode = WrapMode.Loop;
			if(height < 592)
			{
				height += speed * Time.deltaTime;
			}
		}
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
