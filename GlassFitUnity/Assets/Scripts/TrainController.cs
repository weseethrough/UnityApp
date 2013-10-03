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
	private float FakedMovement = -2500f;
	public bool indoor = false;
	
	private Vector3 scale;
	private int originalHeight = 500;
	private int originalWidth = 800;
	
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
		
		var aSources = GetComponents<AudioSource>();
		trainMove = aSources[0];
		trainWhistle = aSources[1];
		
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
		
		trainMove.Play();
	}
	
	void OnEnable() {
		transform.position = new Vector3(103.8f, -6.6f, -50);
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
		
		whistleTime += Time.deltaTime;
		
		if(whistleTime >= 10.0f)
		{
			trainWhistle.Play();
			whistleTime -= 10.0f;
		}
		
		inputData.Poll();
	
		scaledDistance = (inputData.DistanceBehindTarget() - 50) * 76.666f;

		Vector3 movement = new Vector3(103.8f,-108.4f,(float)scaledDistance);
		transform.position = movement;
	}
	
	string SiDistance(double meters) {
		string postfix = "m";
		string final;
		int value = (int)meters;
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
