using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	
	private int zombieID = 0;
	
	private List<GameObject> zombieList;
	
	private GameObject leftMarker;
	private GameObject rightMarker;
	
	private Camera zombieCamera;
	
	public override void Begin ()
	{
		base.Begin ();
		Platform.Instance.GetPlayerOrientation().SetAutoReset(false);
		zombieList = new List<GameObject>();
		AddZombie();
		zombiesKilled = 0;
		leftMarker = GameObject.Find("LeftPointer");
		rightMarker = GameObject.Find("RightPointer");
		GameObject obj = GameObject.Find("ZombieCamera");
		if(obj != null)
		{
			zombieCamera = obj.GetComponent<Camera>();
		}
	}
	
//	void Start()
//	{
//		Platform.Instance.GetPlayerOrientation().SetAutoReset(false);
//		
//		zombieList = new List<GameObject>();
//		AddZombie();
//		zombiesKilled = 0;
//		leftMarker = GameObject.Find("LeftPointer");
//		rightMarker = GameObject.Find("RightPointer");
//		GameObject obj = GameObject.Find("ZombieCamera");
//		if(obj != null)
//		{
//			zombieCamera = obj.GetComponent<Camera>();
//		}
//		UnityEngine.Debug.Log("ZombieSnack: reached the end of Start function");
//	}
	
	// Update is called once per frame
	public override void Update () {
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

		//set visibility of left/right markers based on where zombies are in scnee
		if(zombieList != null && zombieList.Count > 0)
		{
			if(leftMarker != null && rightMarker != null)
			{
				leftMarker.renderer.enabled = false;
				rightMarker.renderer.enabled = false;
			}
			foreach(GameObject zombie in zombieList)
			{
				SkinnedMeshRenderer zombieRenderer = zombie.GetComponentInChildren<SkinnedMeshRenderer>();
				if(zombieCamera != null && zombieRenderer != null) {
					if(!zombieRenderer.isVisible) {
						Vector3 position = zombie.transform.position;
						Vector3 direction = zombieCamera.transform.position - position;
						direction.Normalize();
						direction.y = 0f;
						Vector3 cameraDirection = zombieCamera.transform.forward;
						cameraDirection.y = 0f;
						
						float dotProduct = Vector3.Dot(direction, cameraDirection);
						
						position = zombieCamera.WorldToScreenPoint(position);
						
						if(dotProduct <= 0f)
						{
							position.x = -position.x;
						}
						if(leftMarker != null && position.x > 0)
						{
							leftMarker.renderer.enabled = true;
						}
						else if(rightMarker != null && position.x < 0)
						{
							rightMarker.renderer.enabled = true;
						}
					}
				}
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
			StartCoroutine(ShowBanner(3.0f));
		}
	}
	
	public void AddZombie()
	{
		if(zombiePrefab != null)
		{
			GameObject zombie = (GameObject)Instantiate(zombiePrefab);

			//choose a random position and velocity

			zombie.tag = "Snacks";
			zombie.name = "Zombie" + zombieID.ToString();

			//apply random position
			ZombieController controller = zombie.GetComponent<ZombieController>();
			if(controller != null)
			{
				controller.setRealWorldPos( new Vector3( Random.Range(-2,2), 0, (float)Platform.Instance.LocalPlayerPosition.Distance + 40) );
			}
			//apply speed
			ConstantVelocityPositionController posController = zombie.GetComponent<ConstantVelocityPositionController>();
			if(posController != null)
			{
				posController.setSpeed(speed);
			}

			//housekeeping
			zombieList.Add(zombie);
			numberOfZombies++;
			zombieID++;
			UnityEngine.Debug.Log("Zombie: new zombie added, total is now " + numberOfZombies.ToString());
			DataVault.Set("current_zombies", "Zombies killed: " + zombiesKilled);
		}
		else
		{
			UnityEngine.Debug.Log("Zombie: prefab is null");
		}
	}
	
	public void ReduceNumberOfZombies(string zombieName)
	{
		numberOfZombies--;
		zombiesKilled++;
		foreach(GameObject zombie in zombieList)
		{
			if(zombie.name == zombieName)
			{
				zombieList.Remove(zombie);
				break;
			}
		}
		
		DataVault.Set("current_zombies", "Zombies killed: " + zombiesKilled);
		UnityEngine.Debug.Log("Zombie: zombie removed, total killed is now " + zombiesKilled.ToString());
	}
}
