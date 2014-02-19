using UnityEngine;
using System.Collections;

public class MinigameToken : MonoBehaviour {
	
	float xOffset = 0.0f;
	float yOffset = 0.0f;
	float zDist = 0.0f;
	
	public float rotationSpeed = 10.0f;
		
	// Use this for initialization
	void Start () {
		//get default position
		xOffset = transform.position.x;
		yOffset = transform.position.y;
	}
	
	public void SetDistance(float dist) {
		zDist = dist;
	}
	
	// Update is called once per frame
	void Update () {
		//set position in scene based on player distance and our distance
		float zOffset = zDist - Platform.Instance.GetDistance();
		transform.position = new Vector3(xOffset, yOffset, zOffset);
		
		//spin
		transform.Rotate(0.0f, rotationSpeed*Time.deltaTime, 0.0f);
		
	}
}
