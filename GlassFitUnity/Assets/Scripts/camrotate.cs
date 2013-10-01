using UnityEngine;
using System.Collections;



public class camrotate : MonoBehaviour {

	public Camera MainCam1;
	
	// Use this for initialization
	void Start () {
	Input.gyro.enabled = true;
		
		
	}
	
	// Update is called once per frame
	void Update () {
	
		
		
		MainCam1.transform.rotation =  Input.gyro.attitude;
		
	}
}
