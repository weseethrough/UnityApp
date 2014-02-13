using UnityEngine;
using System.Collections;

public class TargetStartLine : MonoBehaviour {
	
	private float startDistance = 50;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		startDistance -= Time.deltaTime * Platform.Instance.Pace();
		
		transform.position = new Vector3(transform.position.x, 0, startDistance);
	}
}
