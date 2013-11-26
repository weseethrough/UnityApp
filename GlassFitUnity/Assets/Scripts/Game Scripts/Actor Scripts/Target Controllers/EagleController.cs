using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class EagleController : TargetController {

	//private Platform inputData = null;
	
	private float speed;
	private AudioSource screech;
	private bool descend = false;
	
	private Animator anim;
	float screechTime = 0.0f;
	
	// Use this for initialization
	void Start () {
		base.Start();
		anim = GetComponent<Animator>();
		anim.SetBool("Attack", false);
	
		screech = GetComponent<AudioSource>();
	}
	
	void OnEnable() {
		base.OnEnable();
		setAttribs(50, 135, 2092, 0);
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		
		float realDist = (float)target.getDistanceBehindTarget() - distanceOffset;
		
		if(realDist < -49)
		{
			height = 2092;
		}
		
		//UnityEngine.Debug.Log("Eagle: Distance is :" + realDist.ToString());
		if(realDist > -30 && realDist <= 0)
		{
			if(!descend)
			{
				anim.SetBool("Attack", true);
				screech.Play();
				screechTime = 0.0f;
				descend = true;
			}
			
			if(screechTime > 15.0f)
			{
				screechTime -= 15;
				screech.Play();
			}
			
			screechTime += Time.deltaTime;
			
			float time = -realDist / target.getCurrentSpeed();
			speed = height / time;
			if(height > 0)
			{
				height -= speed * Time.deltaTime;
			}
		}
		else
		{
			anim.SetBool("Attack", false);
			descend = false;
			speed = 500;
			if(height < 2092)
			{
				height += speed * Time.deltaTime;
			}
		}
		
	}
}
