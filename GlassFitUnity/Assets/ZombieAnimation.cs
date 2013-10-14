using UnityEngine;
using System.Collections;

public class ZombieAnimation : MonoBehaviour {
	private Platform inputData = null;
	private float speed;
	private Animator anim;
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
		//UnityEngine.Debug.Log("Zombie: getting animator");
		anim = GetComponent<Animator>();
		//UnityEngine.Debug.Log("Zombie: getting speed");
		
		speed = inputData.getCurrentSpeed(0);
		//UnityEngine.Debug.Log("Zombie: setting anim float");
		anim.SetFloat("Speed", speed);
		//UnityEngine.Debug.Log("Zombie: start ok!");
	}
	
	// Update is called once per frame
	void Update () {
		inputData.Poll();
		float newSpeed = inputData.getCurrentSpeed(0);
		if(newSpeed != speed)
		{
			speed = newSpeed;
			anim.SetFloat("Speed", speed);
		}
		
	}
}
