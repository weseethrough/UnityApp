using UnityEngine;
using System.Collections;

public class ThirdPersonController : MonoBehaviour {
	
	private Animator animator;
	
	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		if(animator != null)
		{
			float speed = Platform.Instance.Pace();
			animator.SetFloat("Speed", speed);
			if(speed > 2.2f && speed < 4.0f) {
				animator.speed = speed / 2.2f;
			} else if(speed > 4.0f) {
				animator.speed = Mathf.Clamp(speed / 4.0f, 1, 2);
			} else if(speed > 0.0f) {
				animator.speed = speed / 1.25f;
			} else {
				animator.speed = 1.0f;
			}
		}
		else
		{
			UnityEngine.Debug.LogWarning("ThirdPersonController: Target is null in Start");
		}
	}
	
	void OnEnable()
	{
		animator = GetComponent<Animator>();
		if(animator != null)
		{
			float speed = Platform.Instance.Pace();
			animator.SetFloat("Speed", speed);
			if(speed > 2.2f && speed < 4.0f) {
				animator.speed = speed / 2.2f;
			} else if(speed > 4.0f) {
				animator.speed = Mathf.Clamp(speed / 4.0f, 1, 2);
			} else if(speed > 0.0f) {
				animator.speed = speed / 1.25f;
			} else {
				animator.speed = 1.0f;
			}
		}
		else
		{
			UnityEngine.Debug.LogWarning("ThirdPersonController: Target is null in Start");
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		if(animator != null)
		{
			float speed = Platform.Instance.Pace();
			UnityEngine.Debug.Log("ThirdPersonController: speed is currently " + speed.ToString("f2"));
			animator.SetFloat("Speed", speed);
			if(speed > 2.2f && speed < 4.0f) {
				animator.speed = speed / 2.2f;
			} else if(speed > 4.0f) {
				animator.speed = Mathf.Clamp(speed / 4.0f, 1, 2);
			} else if(speed > 0.0f) {
				animator.speed = speed / 1.25f;
			} else {
				animator.speed = 1.0f;
			}
		}
		else
		{
			UnityEngine.Debug.LogWarning("ThirdPersonController: Target is null in Start");
		}
	}
}
