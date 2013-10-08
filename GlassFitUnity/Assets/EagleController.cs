using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class EagleController : MonoBehaviour {

private float countTime = 3.0f;
	private float trainTime = 0f;
	private float whistleTime = 0.0f;
	private bool started = false;
	private double scaledDistance;
	private Platform inputData = null;
	private float FakedMovement = -2500f;
	public bool indoor = false;
	
	private float speed;
	private AudioSource screech;
	private bool descend = false;
	
	private Animator anim;
	
	private Vector3 scale;
	private int originalHeight = 500;
	private int originalWidth = 800;
	private float height = 592.0f;
	
	private Rect target;	
	private string targetText;
	
	// Background textures
	Texture2D normal;
	Texture2D info;
	Texture2D warning;
	
	public void reset()
	{
		UnityEngine.Debug.Log("Train: position reset");
		//inputData = new Platform();
		scaledDistance = -1000f;
	}
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
		
		target =   new Rect(15, 15, 200, 100);
		
		scale.x = (float)Screen.width / originalWidth;
		scale.y = (float)Screen.height / originalHeight;
    	scale.z = 1;
		
		Color white = new Color(0.9f, 0.9f, 0.9f, 0.5f);
		normal = new Texture2D(1, 1);
		normal.SetPixel(0,0,white);
		normal.Apply();
		
		Color green = new Color(0f, 0.9f, 0f, 0.5f);
		info = new Texture2D(1, 1);
		info.SetPixel(0,0,green);
		info.Apply();
		
		Color red = new Color(0.9f, 0f, 0f, 0.5f);
		warning = new Texture2D(1, 1);
		warning.SetPixel(0,0,red);
		warning.Apply();
		
		UnityEngine.Debug.Log("Eagle: About to get animator");
		
		anim = GetComponent<Animator>();
		UnityEngine.Debug.Log("Eagle: Animator obtained");
		anim.SetBool("Attack", false);
	
		screech = GetComponent<AudioSource>();
		UnityEngine.Debug.Log("Eagle: Attack off");
		
	}
	
	void OnEnable() {
		transform.position = new Vector3(0, height, -50);
		//inputData = new Platform();
		//Debug.Log("OnEnable called\n\n\n\n\n");
	}
	
	void OnGUI() {
				
    	GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		GUI.skin.box.wordWrap = true;
		GUI.skin.box.fontSize = 30;
		GUI.skin.box.fontStyle = FontStyle.Bold;
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;				
		GUI.skin.box.normal.background = normal;
		GUI.skin.box.normal.textColor = Color.black;
		
		// Target Distance
		GUIStyle targetStyle = new GUIStyle(GUI.skin.box);
		double targetDistance = inputData.DistanceBehindTarget()-50;
		if (targetDistance > 0) {
			targetStyle.normal.background = warning; 
			targetText = "Behind!\n";
		} else {
			targetStyle.normal.background = info; 
			targetText = "Ahead\n";
		}
		targetStyle.normal.textColor = Color.white;		
		GUI.Box(target, targetText+"<i>"+SiDistance( Math.Abs(targetDistance) )+"</i>", targetStyle);
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
			UnityEngine.Debug.Log("Eagle: height is: " + height.ToString());
			
			UnityEngine.Debug.Log("Eagle: speed set to: " + speed.ToString());
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
