using UnityEngine;
using System.Collections;

public class TrainingController : TargetController {

	private Animator anim; 
	private float speed;
	private bool started = false;
	private bool move = false;
	private float zMove = 20.0f;
	
	// Use this for initialization
	void Start () {
		//target = Platform.Instance.getTargetTracker();
		SetAttribs(20, 135, -254.6f, 100);
		anim = GetComponent<Animator>();
		
	}
	
	void OnEnable() {
		//base.OnEnable();
		SetAttribs(20, 135, -254.6f, 100);
	}
	
	public void SetMove(bool b) {
		move = b;
	}
	
	void Update () {
				
		if(!started) {
			started = true;
			Platform.Instance.ResetTargets();
			target = Platform.Instance.GetTargetTracker();
			anim.speed = 0.5f;
			target.SetTargetSpeed(2.2f);
		} 
		
		if(move) {
			anim.SetBool("Looking", true);
			
			if(zMove > 0.0f) {
				zMove -= Time.deltaTime * 5.0f;			
				SetAttribs(zMove, 135, -254.6f, 100);
			}
			
			float newSpeed = target.GetCurrentSpeed();
			if(speed != newSpeed)
			{
				speed = newSpeed;
				anim.SetFloat("Speed", speed);
				if(speed >= 2.2f && speed < 4.0f) {
					anim.speed = newSpeed / 2.2f;
				} else if(speed > 4.0f) {
					anim.speed = Mathf.Clamp(newSpeed / 4.0f, 1, 2);
				} else {
					anim.speed = newSpeed / 1.25f;
				}
			}
		}
		
		base.Update();
	}
}
