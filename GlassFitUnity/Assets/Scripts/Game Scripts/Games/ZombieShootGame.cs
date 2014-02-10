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
	
	//private GestureHelper.DownSwipe downHandler = null;
	
	// Use this for initialization
	void Start () {
		AddZombie();
		
		downHandler = new GestureHelper.DownSwipe(() => {
			Application.Quit();
		});
		
		GestureHelper.onSwipeDown += downHandler;
		
		DataVault.Set("countdown_subtitle", " ");
		
		Platform.Instance.SetIndoor(true);
		SetReadyToStart(true);
		
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
			levelUpTime += 45.0f;
			speed += 0.1f;
			if(spawnTime > 3.0f)
			{
				spawnTime -= 0.5f;
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
			DataVault.Set("current_zombies", "Number of zombies: " + numberOfZombies);
		}
		else
		{
			UnityEngine.Debug.Log("Zombie: prefab is null");
		}
	}
	
	
	void OnDestroy()
	{
		GestureHelper.onSwipeDown -= downHandler;
	}
	
	public void ReduceNumberOfZombies()
	{
		numberOfZombies--;
		DataVault.Set("current_zombies", "Number of zombies: " + numberOfZombies);
		UnityEngine.Debug.Log("Zombie: zombie removed, total is now " + numberOfZombies.ToString());
	}
}
