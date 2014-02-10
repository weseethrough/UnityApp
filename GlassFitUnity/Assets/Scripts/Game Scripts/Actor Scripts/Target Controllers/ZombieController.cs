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
	
	AudioSource deathSound;
	
	AudioSource growl;
	
	GameObject marker;
	
	/// <summary>
	/// Start this instance. Sets the attributes
	/// </summary>
	void Start () {
		
		animator = GetComponent<Animator>();
		
		AudioSource[] sounds = GetComponents<AudioSource>();
		
		if(sounds != null) {
			int sound = UnityEngine.Random.Range(0, 3);
			
			if(sound < sounds.Length) {
				growl = sounds[sound];
				growl.Play();
			}
			else
			{
				UnityEngine.Debug.Log("Zombie: not enough sounds for growl!");
			}
			
			sound = UnityEngine.Random.Range(3, 6);
			if(sound < sounds.Length) {
				deathSound = sounds[sound];
			}
			else
			{
				UnityEngine.Debug.Log("Zombie: not enough sounds for death");
			}
		}
		float yRotation = UnityEngine.Random.Range(120f, 240f);
		
		transform.rotation = Quaternion.Euler(0, yRotation, 0);
		
		transform.position = transform.rotation * new Vector3(0, 0, -30);
		
		direction = Vector3.zero - transform.position;
		direction.Normalize();
		
		marker = transform.Find("Pointer").gameObject;
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		// Calculate the position based on the player's position.
		if(!dead) {
			float distanceFromTarget = transform.position.magnitude;
			if(distanceFromTarget > 2.0f)
			{
				float pace = Platform.Instance.Pace();
				transform.position += ((direction * speed) * Time.deltaTime) - ((new Vector3(0, 0, 1) * pace) * Time.deltaTime);
				
				direction = Vector3.zero - transform.position;
				direction.Normalize();
				transform.rotation = Quaternion.LookRotation(direction);
			}
			
			if(distanceFromTarget > 10.0f)
			{
				if(marker != null)
				{
					marker.renderer.enabled = true;
				}
				else
				{
					UnityEngine.Debug.Log("Zombie: pointer is null!");
				}
			}
			else
			{
				if(marker != null)
				{
					marker.renderer.enabled = false;
				}
				else
				{
					UnityEngine.Debug.Log("Zombie: pointer is null!");
				}
			}
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
			
			if(deathSound != null)
			{
				deathSound.Play();
			}
			
			if(marker != null)
			{
				marker.renderer.enabled = false;
			}
			
			StartCoroutine(RemoveDead());
			dead = true;
			speed = 0.0f;
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
