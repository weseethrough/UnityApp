using UnityEngine;
using System.Collections;

public class TrainingController : TargetController {

	private Animator anim; 
	private float speed;
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
		anim.speed = 0.5f;
		target = Platform.Instance.CreateTargetTracker(2.2f);
	}
	
	public void SetSpeed(float speed) {
		anim.speed = speed / 2.2f;
		target = Platform.Instance.CreateTargetTracker(speed);
	}
	
	public void SetMove(bool b) {
		move = b;
	}
	
	void Update () {
		
		if(move) {
			anim.SetBool("Looking", true);
			
			if(zMove > 0.0f) {
				zMove -= Time.deltaTime * 5.0f;			
				SetAttribs(zMove, 135, -254.6f, 100);
			}
			
			float newSpeed = target.PollCurrentSpeed();
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
