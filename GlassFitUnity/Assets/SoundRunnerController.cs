using UnityEngine;
using System.Collections;

public class SoundRunnerController : MonoBehaviour {

	private Platform inputData = null;
	
	private double scaledDistance;
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
	}
	
	void OnEnable() {
		transform.position = new Vector3(-10, -80.8f, (float)scaledDistance);
	}
	
	void OnGUI() {

	}
	
	void Update () {
				
		inputData.Poll();
		
		scaledDistance = (inputData.DistanceBehindTarget() - 50) * 76.666f;
		Vector3 movement = new Vector3(-10, -80.8f,(float)scaledDistance);
		transform.position = movement;
	}
}
