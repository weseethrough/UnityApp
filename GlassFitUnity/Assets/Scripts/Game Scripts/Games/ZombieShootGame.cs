using UnityEngine;
using System.Collections;

public class ZombieShootGame : GameBase {
	
	int numberOfZombies = 0;
	
	public GameObject zombiePrefab;
	
	float currentTime = 0.0f;
	
	float spawnTime = 10.0f;
	
	private float totalTime = 0.0f;
	
	private float levelUpTime = 30.0f;
	
	private float speed = 1.5f;
	
	private int currentLevel = 1;
	private int maxLevel = 10;
	
	private int zombiesKilled = 0;
	
	// Use this for initialization
	void Start () {
		AddZombie();
		
		backHandler = new GestureHelper.OnBack(() => {
			Application.Quit();
		});
		
		GestureHelper.onBack += backHandler;
		
		DataVault.Set("countdown_subtitle", " ");
		
		Platform.Instance.LocalPlayerPosition.SetIndoor(true);
		TriggerUserReady();
		
		finish = 10000;
		
		DataVault.Set("indoor_move", " ");
		
		GraphComponent gc = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        gc.GoToFlow("Zombie Flow");
		
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
			numberOfZombies++;
			UnityEngine.Debug.Log("Zombie: new zombie added, total is now " + numberOfZombies.ToString());
			DataVault.Set("current_zombies", "Zombies killed: " + zombiesKilled);
		}
		else
		{
			UnityEngine.Debug.Log("Zombie: prefab is null");
		}
	}
	
	
	void OnDestroy()
	{
		GestureHelper.onBack -= backHandler;
	}
	
	public void ReduceNumberOfZombies()
	{
		numberOfZombies--;
		zombiesKilled++;
		DataVault.Set("current_zombies", "Zombies killed: " + zombiesKilled);
		UnityEngine.Debug.Log("Zombie: zombie removed, total killed is now " + zombiesKilled.ToString());
	}
}
