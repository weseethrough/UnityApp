using UnityEngine;
using System.Collections;

public class MinigameToken : RYWorldObject {
	
	public float rotationSpeed = 10.0f;
		
	// Use this for initialization
	void Start () {
		base.Start();
		UnityEngine.Debug.LogError("minigame token start");
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		//spin
		transform.Rotate(0.0f, rotationSpeed*Time.deltaTime, 0.0f);
		
	}
}
