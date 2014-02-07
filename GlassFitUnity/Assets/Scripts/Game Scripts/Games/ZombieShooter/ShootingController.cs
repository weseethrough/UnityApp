using UnityEngine;
using System.Collections;

public class ShootingController : MonoBehaviour {

	GestureHelper.OnTap tapHandler = null;
	
	ZombieShootGame game;
	
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
//						if(game != null)
//						{
//							game.ReduceNumberOfZombies();
//						}
//						else
//						{
//							UnityEngine.Debug.Log("Shooter: game is null!");
//						}
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
					zombie.LoseHealth();
					if(zombie.IsDead())
					{
//						if(game != null)
//						{
//							game.ReduceNumberOfZombies();
//						}
//						else
//						{
//							UnityEngine.Debug.Log("Shooter: game is null!");
//						}
					}
				} 
				else
				{
					UnityEngine.Debug.Log("Shooter: zombie not found!");
				}
			}
		}
	}
	
	void OnDestroy() {
		GestureHelper.onTap -= tapHandler;
	}
}
