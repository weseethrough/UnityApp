using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

/// <summary>
/// Controls the position of the zombies
/// </summary> 
public class ZombieController : MonoBehaviour {
	
	int health = 3;
	
	private Animator animator;
	
	private GestureHelper.DownSwipe downHandler = null;
	
	Vector3 direction;
	
	bool dead = false;
	
	float speed = 2.4f;
	
	/// <summary>
	/// Start this instance. Sets the attributes
	/// </summary>
	void Start () {
		
		animator = GetComponent<Animator>();
		
		downHandler = new GestureHelper.DownSwipe(() => {
			Application.Quit();
		});
		
		GestureHelper.onSwipeDown += downHandler;
		
		float yRotation = UnityEngine.Random.Range(0f, 360f);
		
		transform.rotation = Quaternion.Euler(0, yRotation, 0);
		
		transform.position = transform.rotation * new Vector3(0, 0, -30);
		
		direction = Vector3.zero - transform.position;
		direction.Normalize();
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		// Update the base.
		transform.position += (direction * speed) * Time.deltaTime;
	}
	
	public void SetDead() {
		
		if(animator != null && !dead)
		{
			animator.SetBool("Dead", true);
			StartCoroutine(RemoveDead());
			dead = true;
		}
	
	}
	
	IEnumerator RemoveDead()
	{
		yield return new WaitForSeconds(3.0f);
		
		Destroy(transform.gameObject);
	}
	
	public void LoseHealth() {
		health -= 1;
		if(health == 0)
		{
			SetDead();
		}
	}
	
	public bool IsDead()
	{
		return dead;
	}
	
	void OnDestroy()
	{
		GestureHelper.onSwipeDown -= downHandler;
	}
	
	public void SetSpeed(float s)
	{
		speed = s;
	}
}
