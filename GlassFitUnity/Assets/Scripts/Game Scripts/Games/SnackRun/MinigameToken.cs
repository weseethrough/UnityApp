using UnityEngine;
using System.Collections;
using RaceYourself;

public class MinigameToken : WorldObject {
	
	public float rotationSpeed = 10.0f;
		
	// Use this for initialization
	public override void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();
		//spin
		transform.Rotate(0.0f, rotationSpeed*Time.deltaTime, 0.0f);
		
	}
}
