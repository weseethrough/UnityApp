using UnityEngine;
using System.Collections;

public class SoundRunnerController : MonoBehaviour {

	private double scaledDistance;
	
	// Use this for initialization
	void Start () {
	}
	
	void OnEnable() {
		transform.position = new Vector3(-10, -80.8f, (float)scaledDistance);
	}
	
	void OnGUI() {

	}
	
	void Update () {
				
		Platform.Instance.Poll();
		
		scaledDistance = (Platform.Instance.DistanceBehindTarget() - 50) * 76.666f;
		Vector3 movement = new Vector3(-10, -80.8f,(float)scaledDistance);
		transform.position = movement;
	}
}
