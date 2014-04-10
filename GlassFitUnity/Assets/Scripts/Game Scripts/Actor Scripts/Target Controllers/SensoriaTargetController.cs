using UnityEngine;
using System.Collections;

// NOTE this class won't currently work. Refactor required to use position controller - AH


public class SensoriaTargetController : TargetController {

	private Animator anim; 
	private float speed;
	
	// Use this for initialization
	public override void Start () {

		UnityEngine.Debug.LogError("Using deprecated class. Refactor required");
		base.Start();
		anim = GetComponent<Animator>();
		UnityEngine.Debug.Log("SensTarget: animator obtained");
		speed = 2.2f;
		anim.SetFloat("Speed", 2.3f);
	}

	
	// Update is called once per frame
	public override void Update () {
		base.Update();
		
		if(Platform.Instance.GetHighestDistBehind() > 10 && speed > 0.0f) {
			UnityEngine.Debug.Log("SensTarget: distance reached, lowering speed");
			speed = 0.0f;
		} else if(Platform.Instance.GetHighestDistBehind() < 10 && speed < 2.3f) {
			UnityEngine.Debug.Log("SensTarget: distance reached, increasing speed");
			speed = 2.3f;
		}
	}
}
