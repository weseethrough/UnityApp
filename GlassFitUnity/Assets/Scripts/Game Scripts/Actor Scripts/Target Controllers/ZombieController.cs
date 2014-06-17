using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using RaceYourself;

/// <summary>
/// Controls the position of the zombies
/// </summary> 
public class ZombieController : WorldObject {
	
	int health = 3;
	
	private Animator animator;
	
	Vector3 direction;
	
	bool dead = false;
	
	float speed = 2.4f;
	
	AudioSource deathSound;
	
	AudioSource growl;
	
	GameObject marker;
	
	new CapsuleCollider collider;
	
	float nearPlayerTime = 0.0f;
	
	bool nearPlayer = false;
	ConstantVelocityPositionController posController = null;
	
	/// <summary>
	/// Start this instance. Sets the attributes
	/// </summary>
	public override void Start () {

		base.Start();

		animator = GetComponent<Animator>();
		posController = GetComponent<ConstantVelocityPositionController>();

		//init sounds
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

		direction = Vector3.zero - transform.position;
		direction.Normalize();

		posController.setDir(direction);

		marker = transform.Find("Pointer").gameObject;
		
		collider = GetComponentInChildren<CapsuleCollider>();

	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	public override void Update () {
		//call base update, which sets scene position based on world position
		base.Update();
		// Calculate the position based on the player's position.
		if(!dead) {
			float distanceFromTarget = transform.position.magnitude;
			if(distanceFromTarget > 2.0f)
			{
				//unfreeze
				setScenePositionFrozen(false);

				//face the player
				direction = Vector3.zero - transform.position;
				direction.Normalize();
				transform.rotation = Quaternion.LookRotation(direction);
				//move towards the player
				posController.setDir(direction);

				//reset timer
				nearPlayerTime = 0.0f;
			}
			else
			{
				//within range. Freeze position
				setScenePositionFrozen(true);

				//increment fail timer and check for fail
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

			//scale target collider based on distance
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
			posController.setSpeed(0);
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
}
