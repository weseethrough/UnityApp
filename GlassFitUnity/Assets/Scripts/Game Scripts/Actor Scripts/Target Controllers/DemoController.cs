using UnityEngine;
using System.Collections;

public class DemoController : TargetController {

	private Animator anim; 
	private float speed;
	private bool started = false;
	
	// Use this for initialization
	void Start () {
		//target = Platform.Instance.getTargetTracker();
		base.setAttribs(0, 135, -254.6f, 50);
		anim = GetComponent<Animator>();
		
	}
	
	void OnEnable() {
		//base.OnEnable();
		base.setAttribs(0, 135, -254.6f, 50);
	}
	
	void Update () {
				
		if(!started) {
			started = true;
			Platform.Instance.resetTargets();
			target = Platform.Instance.getTargetTracker();
			anim.speed = target.getCurrentSpeed() / 2.2f;
			speed = target.getCurrentSpeed();
			anim.SetFloat("Speed", speed);
		}
		
		base.Update();
		float newSpeed = target.getCurrentSpeed();
		if(speed != newSpeed)
		{
			speed = newSpeed;
			anim.SetFloat("Speed", speed);
			if(speed > 2.2f && speed < 4.0f) {
				anim.speed = newSpeed / 2.2f;
			} else if(speed > 4.0f) {
				anim.speed = Mathf.Clamp(newSpeed / 4.0f, 1, 2);
			} else {
				anim.speed = newSpeed / 1.25f;
			}
		}
	}
}
