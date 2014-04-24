using UnityEngine;
using System.Collections;

public class ThirdPersonCyclistAnimationController : MonoBehaviour {

	public float speed;
	private Animator animator;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
	}

	void OnEnable() {
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		if(animator != null) {
			if(Platform.Instance.LocalPlayerPosition.Pace > 0) {
				animator.speed = 1.0f;
			} else {
				animator.speed = 0.0f;
			}
		} else {
			UnityEngine.Debug.Log("ThirdPersonCyclistAnimationController: No animator found!");
		}
	}
}
