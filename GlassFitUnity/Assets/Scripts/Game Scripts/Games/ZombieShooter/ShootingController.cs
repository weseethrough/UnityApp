using UnityEngine;
using System.Collections;

public class ShootingController : MonoBehaviour {

	GestureHelper.OnTap tapHandler = null;
	
	ZombieShootGame game;
	
	float reloadTime = 0.0f;
	
	// Use this for initialization
	void Start () {
		tapHandler = new GestureHelper.OnTap(() => {
			CheckCollisions();
		});
		
		GestureHelper.onTap += tapHandler;
		
		GameObject obj = GameObject.Find("ZombieGUI");
		if(obj != null)
		{
			game = obj.GetComponent<ZombieShootGame>();
			if(game == null)
			{
				UnityEngine.Debug.Log("Shooter: game not found!");
			}
		}
		else
		{
			UnityEngine.Debug.Log("Shooter: ZombieGUI object not found!");
		}
	}
	
	void CheckCollisions() 
	{
		RaycastHit hit;
		
		if(Physics.Raycast(transform.position, transform.forward, out hit))
		{
			if(hit.collider.tag == "Head")
			{
				UnityEngine.Debug.Log("Shooter: Boom - headshot!");
				GameObject zombiePart = hit.transform.gameObject;
				ZombieController zombie = zombiePart.transform.root.GetComponent<ZombieController>();
				if(zombie != null) {
					if(!zombie.IsDead()) {
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
				UnityEngine.Debug.Log("Shooter: Boom - headshot!");
				reloadTime += Time.deltaTime;
				if(reloadTime > 1.0f)
				{
					GameObject zombiePart = hit.transform.gameObject;
					ZombieController zombie = zombiePart.transform.root.GetComponent<ZombieController>();
					if(zombie != null) {
						if(!zombie.IsDead()) {
							zombie.SetDead();
							reloadTime = 0.0f;
						}
					} 
					else
					{
						UnityEngine.Debug.Log("Shooter: zombie not found!");
					}
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
	
	void Update()
	{
		CheckLoadingCollisions();
	}
	
	void OnDestroy() {
		GestureHelper.onTap -= tapHandler;
	}
}
