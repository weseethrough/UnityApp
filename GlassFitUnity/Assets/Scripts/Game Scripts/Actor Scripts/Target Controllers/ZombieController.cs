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
	
	CapsuleCollider collider;
	
	float nearPlayerTime = 0.0f;
	
	bool nearPlayer = false;
	
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
		float yRotation = UnityEngine.Random.Range(150f, 210f);
		
		transform.rotation = Quaternion.Euler(0, yRotation, 0);
		
		transform.position = transform.rotation * new Vector3(0, 0, -30);
		
		direction = Vector3.zero - transform.position;
		direction.Normalize();
		
		marker = transform.Find("Pointer").gameObject;
		
		collider = GetComponentInChildren<CapsuleCollider>();
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
				float pace = Platform.Instance.LocalPlayerPosition.Pace;
				transform.position += ((direction * speed) * Time.deltaTime) - ((new Vector3(0, 0, 1) * pace) * Time.deltaTime);
				
				direction = Vector3.zero - transform.position;
				direction.Normalize();
				transform.rotation = Quaternion.LookRotation(direction);
				nearPlayerTime = 0.0f;;
			}
			else
			{
				nearPlayerTime += Time.deltaTime;
				if(nearPlayerTime > 2.0f)
				{
					GameObject gui = GameObject.Find("ZombieGUI");
					if(gui != null)
					{
						ZombieSnack snack = gui.GetComponent<ZombieSnack>();
						if(snack != null)
						{
							snack.EndGame();
						}
					}
				}
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
				
				if(collider != null)
				{
					collider.transform.localScale = new Vector3(1.069066f, 1.837455f, 0.5571659f);
				}
				else
				{
					UnityEngine.Debug.Log("Zombie: collider is null!");
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
				
				if(collider != null)
				{
					collider.transform.localScale = new Vector3(0.4809462f, 1.430881f, 0.4338819f);
				}
				else
				{
					UnityEngine.Debug.Log("Zombie: collider is null!");
				}
			}
		}
	}
		
	public void SetDead() {
		
		if(animator != null && !dead)
		{
			animator.SetBool("Dead", true);
			
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
