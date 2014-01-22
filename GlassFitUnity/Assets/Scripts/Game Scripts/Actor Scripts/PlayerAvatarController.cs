using UnityEngine;
using System.Collections;

public class PlayerAvatarController : MonoBehaviour {
	
	private Animator anim; 

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();	
	}
	
	// Update is called once per frame
	void Update () {
		//update anim to match
		anim = GetComponent<Animator>();
		float speed = Platform.Instance.Pace();
		anim.SetFloat("Speed", speed);
		if(speed > 2.2f && speed < 4.0f) {
			anim.speed = speed / 2.2f;
		} else if(speed > 4.0f) {
			anim.speed = Mathf.Clamp(speed / 4.0f, 1, 2);
		} else {
			anim.speed = speed / 1.0f;
		}
	}
}
