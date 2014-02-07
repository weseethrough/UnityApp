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
	
	Vector3 direction;
	
	bool dead = false;
	
	float speed = 2.4f;
	
	/// <summary>
	/// Start this instance. Sets the attributes
	/// </summary>
	void Start () {
		
		animator = GetComponent<Animator>();
		
		float yRotation = UnityEngine.Random.Range(120f, 240f);
		
		transform.rotation = Quaternion.Euler(0, yRotation, 0);
		
		transform.position = transform.rotation * new Vector3(0, 0, -30);
		
		direction = Vector3.zero - transform.position;
		direction.Normalize();
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		// Calculate the position based on the player's position.
		if(!dead) {
			transform.position += ((direction * speed) * Time.deltaTime) - ((new Vector3(0, 0, 1) * Platform.Instance.Pace()) * Time.deltaTime);
			direction = Vector3.zero - transform.position;
			direction.Normalize();
			transform.rotation = Quaternion.LookRotation(direction);
		}
	}
		
	public void SetDead() {
		
		if(animator != null && !dead)
		{
			animator.SetBool("Dead", true);
			
			GameObject obj = GameObject.Find("ZombieGUI");
			if(obj != null)
			{
				ZombieShootGame game = obj.GetComponent<ZombieShootGame>();
				if(game != null)
				{
					game.ReduceNumberOfZombies();
				}
				else
				{
					UnityEngine.Debug.Log("Shooter: game not found!");
				}
			}
			else
			{
				UnityEngine.Debug.Log("Shooter: ZombieGUI object not found!");
			}
			
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
	
	public void SetSpeed(float s)
	{
		speed = s;
	}
}
