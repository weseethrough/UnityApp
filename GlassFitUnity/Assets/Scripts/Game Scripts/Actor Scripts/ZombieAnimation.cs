using UnityEngine;
using System.Collections;

public class ZombieAnimation : MonoBehaviour {
	private float speed;
	private Animator anim;
	private TargetController controller;
	
	// Use this for initialization
	void Start () {
		//UnityEngine.Debug.Log("Zombie: getting animator");
		anim = GetComponent<Animator>();
		//UnityEngine.Debug.Log("Zombie: getting speed");
		controller = transform.parent.gameObject.GetComponent<TargetController>();
		
		speed = controller.target.PollCurrentSpeed();
		//UnityEngine.Debug.Log("Zombie: setting anim float");
		anim.SetFloat("Speed", speed);
		if(speed > 2.2f) {
				anim.speed = Mathf.Clamp(speed / 4.0f, 1, 2.5f);
			} else {
				anim.speed = speed / 1.25f;
		}
		//UnityEngine.Debug.Log("Zombie: start ok!");
	}
	
	// Update is called once per frame
	void Update () {
		Platform.Instance.Poll();
		float newSpeed = controller.target.PollCurrentSpeed();
		if(newSpeed != speed)
		{
			speed = newSpeed;
			anim.SetFloat("Speed", speed);
			if(speed > 2.2f) {
				anim.speed = Mathf.Clamp(speed / 4.0f, 1, 2.0f);
			} else {
				anim.speed = speed / 1.25f;
			}
		}
		
	}
}
