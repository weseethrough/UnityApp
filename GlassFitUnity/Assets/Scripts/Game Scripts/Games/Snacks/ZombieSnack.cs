using UnityEngine;
using System.Collections;

public class ZombieSnack : SnackBase {
	
	int numberOfZombies = 0;
	
	public GameObject zombiePrefab;
	
	float currentTime = 0.0f;
	
	float spawnTime = 10.0f;
	
	private float totalTime = 0.0f;
	
	private float levelUpTime = 30.0f;
	
	private float speed = 1.5f;
	
	private int currentLevel = 1;
	private int maxLevel = 10;
	
	private int zombiesKilled;
	
	private bool ending = false;
	
	public override void Begin ()
	{
		base.Begin ();
		Platform.Instance.GetPlayerOrientation().SetAutoReset(false);
		AddZombie();
		zombiesKilled = 0;
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		
		currentTime += Time.deltaTime;
		totalTime += Time.deltaTime;
		
		if(totalTime > levelUpTime && currentLevel < maxLevel)
		{
			levelUpTime += 10.0f;
			speed += 0.5f;
			if(spawnTime > 2.0f)
			{
				spawnTime -= 2.0f;
			}
			currentLevel++;
		}
		
		if(currentTime > spawnTime)
		{
			currentTime -= spawnTime;
			if(numberOfZombies < 6)
			{
				UnityEngine.Debug.Log("Zombie: adding a zombie");
				AddZombie();
			}
		}
		
	}
	
	public void EndGame()
	{
		if(!ending){
			ending = true;
			DataVault.Set("death_colour", "EA0000FF");
			DataVault.Set("snack_result", "You died!");
			DataVault.Set("snack_result_desc", "you killed " + zombiesKilled.ToString() + " zombies!");
			UnityEngine.Debug.Log("ZombieSnack: you killed " + zombiesKilled.ToString() + " zombies!");
			finish = true;
			Platform.Instance.GetPlayerOrientation().SetAutoReset(true);
			StartCoroutine(ShowBanner());
		}
	}
	
	public void AddZombie()
	{
		if(zombiePrefab != null)
		{
			GameObject zombie = (GameObject)Instantiate(zombiePrefab);
			zombie.tag = "Snacks";
			ZombieController controller = zombie.GetComponent<ZombieController>();
			if(controller != null)
			{
				controller.SetSpeed(speed);
			}
			numberOfZombies++;
			UnityEngine.Debug.Log("Zombie: new zombie added, total is now " + numberOfZombies.ToString());
			DataVault.Set("current_zombies", "Zombies killed: " + zombiesKilled);
		}
		else
		{
			UnityEngine.Debug.Log("Zombie: prefab is null");
		}
	}
	
	public void ReduceNumberOfZombies()
	{
		numberOfZombies--;
		zombiesKilled++;
		DataVault.Set("current_zombies", "Zombies killed: " + zombiesKilled);
		UnityEngine.Debug.Log("Zombie: zombie removed, total killed is now " + zombiesKilled.ToString());
	}
}
