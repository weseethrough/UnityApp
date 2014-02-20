using UnityEngine;
using System.Collections;

public class ShootingControllerSnack : MonoBehaviour {

	GestureHelper.OnTap tapHandler = null;
	
	ZombieSnack game;
	
	float reloadTime = 0.0f;
	float fireTime = 0.75f;
	
	ShrinkingReticle shootingIcon;
	
	GameObject reticle;
	
	AudioSource gunshot;
	
	UITexture reticleTexture;
	
	// Use this for initialization
	void Start () {
		tapHandler = new GestureHelper.OnTap(() => {
			CheckCollisions();
		});
		
		//GestureHelper.onTap += tapHandler;
		
		GameObject obj = GameObject.Find("ZombieGUI");
		if(obj != null)
		{
			game = obj.GetComponent<ZombieSnack>();
			if(game == null)
			{
				UnityEngine.Debug.Log("Shooter: game not found!");
			}
		}
		else
		{
			UnityEngine.Debug.Log("Shooter: ZombieGUI object not found!");
		}
		
		obj = GameObject.Find("LifeBar");
		if(obj != null)
		{
			shootingIcon = obj.GetComponent<ShrinkingReticle>();
			if(shootingIcon == null)
			{
				UnityEngine.Debug.Log("Shooter: icon not found!");
			}
		}
		else
		{
			UnityEngine.Debug.Log("Shooter: icon object not found!");
		}
		
		gunshot = GetComponent<AudioSource>();
		
		reticle = GameObject.Find("Crosshair");
		if(reticle != null)
		{
			reticleTexture = reticle.GetComponent<UITexture>();
			if(reticleTexture == null)
			{
				UnityEngine.Debug.Log("Shooter: Can't find UI texture");
			}
		}
		else
		{
			UnityEngine.Debug.Log("Shooter: Can't find the crosshair!");
		}
	}
	
	void CheckCollisions() 
	{
		RaycastHit hit;
		
		if(Physics.Raycast(transform.position, transform.forward, out hit))
		{
			if(gunshot != null)
			{
				gunshot.Play();
			}
			
			if(hit.collider.tag == "Head")
			{
				UnityEngine.Debug.Log("Shooter: Boom - headshot!");
				GameObject zombiePart = hit.transform.gameObject;
				ZombieController zombie = zombiePart.transform.root.GetComponent<ZombieController>();
				if(zombie != null) {
					if(!zombie.IsDead()) {
						if(game != null)
						{
							game.ReduceNumberOfZombies();
						}
						else
						{
							UnityEngine.Debug.Log("Shooter: game not found!");
						}
						zombie.SetDead();
					}
				} 
				else
				{
					UnityEngine.Debug.Log("Shooter: zombie not found!");
				}
			}
			else if(hit.collider.tag == "Body")
			{
				UnityEngine.Debug.Log("Shooter: Shot to the body");
				
				GameObject zombiePart = hit.transform.gameObject;
				ZombieController zombie = zombiePart.transform.root.GetComponent<ZombieController>();
				if(zombie != null) {
					if(!zombie.IsDead()) {
						zombie.LoseHealth();
					}
				} 
				else
				{
					UnityEngine.Debug.Log("Shooter: zombie not found!");
				}
			}
			else
			{
				reloadTime = 0.0f;
			}
		}
		else
		{
			reloadTime = 0.0f;
		}
	}
	
	void CheckLoadingCollisions() 
	{
		RaycastHit hit;
		
		if(Physics.Raycast(transform.position, transform.forward, out hit))
		{
			if(hit.collider.tag == "Head" || hit.collider.tag == "Body")
			{
				UnityEngine.Debug.Log("Shooter: looking at one of the ugly SOB's!");
								
				GameObject zombiePart = hit.transform.gameObject;
				ZombieController zombie = zombiePart.transform.root.GetComponent<ZombieController>();
				
				if(!zombie.IsDead()) {
					UnityEngine.Debug.Log("Shooter: he alive, kill it quick!");
					ChangeReticleColour(Color.red);
					if(reloadTime == 0.0f)
					{
						if(shootingIcon != null)
						{
							UnityEngine.Debug.Log("Shooter: starting turning");
							shootingIcon.StartTurning();
						}
						else
						{
							UnityEngine.Debug.Log("Shooter: our shooting icon is null");
						}
					}
					reloadTime += Time.deltaTime;
					if(reloadTime > fireTime)
					{
						if(zombie != null) {
							if(game != null)
							{
								game.ReduceNumberOfZombies();
							}
							else
							{
								UnityEngine.Debug.Log("Shooter: game not found!");
							}
							zombie.SetDead();
							reloadTime = 0.0f;
							if(shootingIcon != null)
							{
								shootingIcon.StopTurning();
							}
							if(gunshot != null)
							{
								gunshot.Play();
							}
							Destroy(hit.collider);
							StartCoroutine(PopReticle());
						} 
						else
						{
							UnityEngine.Debug.Log("Shooter: zombie not found!");
						}
					}
				}
				else
				{
					UnityEngine.Debug.Log("Shooter: zed's dead baby, zed's dead");
				}
			}
			else
			{
				reloadTime = 0.0f;
				if(shootingIcon != null)
				{
					ChangeReticleColour(Color.white);
					shootingIcon.StopTurning();
				}
			}
		}
		else
		{
			reloadTime = 0.0f;
			if(shootingIcon != null)
			{
				ChangeReticleColour(Color.white);
				shootingIcon.StopTurning();
			}
		}
	}
	
	void Update()
	{
		CheckLoadingCollisions();
	}
	
	void OnDestroy() {
		GestureHelper.onTap -= tapHandler;
	}
	
	void ChangeReticleColour(Color color)
	{
		if(reticleTexture != null)
		{
			reticleTexture.color = color;
		}
		else
		{
			UnityEngine.Debug.Log("Shooter: reticle is null, finding again");
			reticle = GameObject.Find("Crosshair");
			if(reticle != null)
			{
				reticleTexture = reticle.GetComponent<UITexture>();
				if(reticleTexture == null)
				{
					UnityEngine.Debug.Log("Shooter: Can't find UI texture");
				}
			}
			else
			{
				UnityEngine.Debug.Log("Shooter: Can't find the crosshair!");
			}
		}
	}
	
	IEnumerator PopReticle()
	{
		reticle.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
		yield return null;
		reticle.transform.localScale = Vector3.one;
	}
}
