using UnityEngine;
using System.Collections;

public class SensoriaTargetController : TargetController {

	private Animator anim; 
	private float speed;
	
	// Use this for initialization
	void Start () {
		base.Start();
		base.SetAttribs(0, 135, -254.6f, 50);
		
		anim = GetComponent<Animator>();
		UnityEngine.Debug.Log("SensTarget: animator obtained");
		speed = 2.2f;
		anim.SetFloat("Speed", 2.3f);
	}
	
	void OnEnable() {
		base.OnEnable();
		base.SetAttribs(0, 135, -254.6f, 50);
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		
		if(Platform.Instance.GetHighestDistBehind() > 10 && speed > 0.0f) {
			UnityEngine.Debug.Log("SensTarget: distance reached, lowering speed");
			speed = 0.0f;
			SetSpeed(speed);
		} else if(Platform.Instance.GetHighestDistBehind() < 10 && speed < 2.3f) {
			UnityEngine.Debug.Log("SensTarget: distance reached, increasing speed");
			speed = 2.3f;
			SetSpeed(speed);
		}
	}
}
