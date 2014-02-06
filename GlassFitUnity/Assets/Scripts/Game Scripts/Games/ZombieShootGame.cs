using UnityEngine;
using System.Collections;

public class ZombieShootGame : MonoBehaviour {
	
	int numberOfZombies = 0;
	
	public GameObject zombiePrefab;
	
	float currentTime = 0.0f;
	
	float spawnTime = 10.0f;
	
	private float totalTime = 0.0f;
	
	private float levelUpTime = 30.0f;
	
	private float speed = 2.4f;
	
	// Use this for initialization
	void Start () {
		AddZombie();
	}
	
	// Update is called once per frame
	void Update () {
		currentTime += Time.deltaTime;
		totalTime += Time.deltaTime;
		
		if(totalTime > levelUpTime)
		{
			levelUpTime += 30.0f;
			speed += 0.5f;
			if(spawnTime > 2.0f)
			{
				spawnTime -= 1.0f;
			}
		}
		
		if(currentTime > spawnTime)
		{
			currentTime -= spawnTime;
			if(numberOfZombies < 20)
			{
				AddZombie();
			}
		}
		
	}
	
	public void AddZombie()
	{
		if(zombiePrefab != null)
		{
			GameObject zombie = (GameObject)Instantiate(zombiePrefab);
			ZombieController controller = zombie.GetComponent<ZombieController>();
			if(controller != null)
			{
				controller.SetSpeed(speed);
			}
			numberOfZombies += 1;
			UnityEngine.Debug.Log("Zombie: new zombie added, total is now " + numberOfZombies.ToString());
			DataVault.Set("current_zombies", numberOfZombies);
		}
	}
	
	public void ReduceNumberOfZombies()
	{
		numberOfZombies -= 1;
		DataVault.Set("current_zombies", numberOfZombies);
		UnityEngine.Debug.Log("Zombie: zombie removed, total is now " + numberOfZombies.ToString());
	}
}
