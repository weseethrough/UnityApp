using UnityEngine;
using System.Collections;

public class SettingsScreen : MonoBehaviour {
	public bool menuOpen = false;
	public bool runner = false;
	public bool eagle = false;
	public bool train = false;
	public bool zombie = false;
	private bool changed = false;
	public bool indoor = true;
	
	private Platform inputData = null;
	
	public GameObject eagleHolder;
	public GameObject runnerHolder;
	public GameObject zombieHolder;
	public GameObject trainHolder;
	
	private int originalHeight = 500;
	private int originalWidth = 800;
	private Vector3 scale;
	
	private bool started = false;
	private bool countdown = false;
	private float countTime = 3.0f;
	public float targSpeed = 1.8f;
	public Texture blackTexture;
	
	private string indoorText = "Indoor Active";
	
	void OnGUI() {
		
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		GUI.skin.button.fontSize = 15;
		GUI.skin.label.fontSize = 15;
		GUI.skin.horizontalSliderThumb.fixedWidth = 30;
		GUI.skin.horizontalSliderThumb.fixedHeight = 30;
		
		if(menuOpen)
		{
			GUI.DrawTexture(new Rect(0,0,originalWidth,originalHeight), blackTexture);
					
			if(GUI.Button(new Rect(0, originalHeight-200, 200, 200) , "Runner"))
			{
				runner = true;
				eagle = false;
				zombie = false;
				train = false;
				
				changed = true;
				
				zombieHolder.SetActive(zombie);
				eagleHolder.SetActive(eagle);
				runnerHolder.SetActive(runner);
				trainHolder.SetActive(train);
			}
			
			if(GUI.Button(new Rect (200, originalHeight-200, 200, 200), "Eagle"))
			{
				runner = false;
				eagle = true;
				zombie = false;
				train = false;
				changed = true;
				
				zombieHolder.SetActive(zombie);
				eagleHolder.SetActive(eagle);
				runnerHolder.SetActive(runner);
				trainHolder.SetActive(train);
			}
			
			if(GUI.Button(new Rect (400, originalHeight-200, 200, 200), "Zombie"))
			{
				runner = false;
				eagle = false;
				zombie = true;
				train = false;
				changed = true;
				
				zombieHolder.SetActive(zombie);
				eagleHolder.SetActive(eagle);
				runnerHolder.SetActive(runner);
				trainHolder.SetActive(train);
			}
			
			if(GUI.Button(new Rect(600, originalHeight-200, 200, 200), "Train"))
			{
				runner = false;
				eagle = false;
				zombie = false;
				train = true;
				changed = true;
				
				zombieHolder.SetActive(zombie);
				eagleHolder.SetActive(eagle);
				runnerHolder.SetActive(runner);
				trainHolder.SetActive(train);
			}
			
			
			if(GUI.Button(new Rect(0, 0, 100, 100), indoorText))
			{
				if(indoor) {
					indoor = false;
					UnityEngine.Debug.Log("Outdoor mode active");
					indoorText = "Outdoor Active";
					changed = true;
				}
				else {
					indoor = true;
					UnityEngine.Debug.Log("Indoor mode active");
					indoorText = "Indoor Active";
					changed = true;
				}
			}
			
			float temp  = GUI.HorizontalSlider(new Rect((originalWidth/2)-100, 250, 200, 50), targSpeed,  1.4f, 2.8f);
    		GUI.Label(new Rect(originalWidth/2 + 120, 250, 100, 50), temp.ToString("f2"));
			if(temp != targSpeed)
			{
				changed = true;
				targSpeed = temp;
			}
	
			if (GUI.Button(new Rect(10, ((originalHeight)/2)-50, 100, 50), "Back"))
			{
        	    menuOpen = false;
				
				if(changed) {
					inputData.stopTrack();
					inputData.reset();
					inputData.setTargetSpeed(targSpeed);
					inputData.setIndoor(indoor);
					//inputData = new Platform();
					
					eagleHolder.GetComponent<EagleController>().indoor = indoor;
					runnerHolder.GetComponent<PBRunnerController>().indoor = indoor;
					zombieHolder.GetComponent<ZombieController>().indoor = indoor;
					trainHolder.GetComponent<TrainController>().indoor = indoor;
					
					eagleHolder.GetComponent<EagleController>().reset();
					runnerHolder.GetComponent<PBRunnerController>().reset();
					zombieHolder.GetComponent<ZombieController>().reset();
					trainHolder.GetComponent<TrainController>().reset();
					
					started = false;
					countdown = false;
					countTime = 3.0f;
					
					UnityEngine.Debug.LogWarning("Platform: Count time is: " + countTime.ToString());
					changed = false;
				}
			}
		} 
		else
		{
			if (GUI.Button(new Rect(10, ((originalHeight)/2)-50, 100, 50), "Options")){
        		menuOpen = true;
				
			}
		}
		
		if(countdown)
		{
			GUI.skin.label.fontSize = 40;
			//float currentTime = 3.0f - timer.Elapsed.Seconds;
			int cur = Mathf.CeilToInt(countTime);
			if(countTime > 0.0f)
			{
				GUI.Label(new Rect(400, 200, 200, 200), cur.ToString()); 
			}
			else if(countTime > -1.0f && countTime < 0.0f)
			{
				GUI.Label(new Rect(400, 200, 200, 200), "GO!"); 
			}
		}
	}
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
		inputData.setIndoor(indoor);
		float x = (float)Screen.width/originalWidth;
		float y = (float)Screen.height/originalHeight;
		scale = new Vector3(x, y, 1);
		
		if(eagle)
		{
			eagleHolder.SetActive(true);
		}
		else if(runner)
		{
			runnerHolder.SetActive(true);
		}
		else if(zombie)
		{
			zombieHolder.SetActive(true);
		}
		else if(train)
		{
			trainHolder.SetActive(true);
		}
		
		eagleHolder.GetComponent<EagleController>().indoor = indoor;
		runnerHolder.GetComponent<PBRunnerController>().indoor = indoor;
		zombieHolder.GetComponent<ZombieController>().indoor = indoor;
		trainHolder.GetComponent<TrainController>().indoor = indoor;
	}
	
	// Update is called once per frame
	void Update () {
		
		if(inputData.hasLock() || indoor)
		{
			countdown = true;
			//UnityEngine.Debug.LogWarning("Platform: In countdown loop");
		 	if(countTime <= -1.0f && !started)
			{
				inputData.StartTrack(indoor);
				UnityEngine.Debug.LogWarning("Tracking Started");
				started = true;
			}
			else if(countTime > -1.0f)
			{
				UnityEngine.Debug.LogWarning("Counting Down");
				countTime -= Time.deltaTime;
			}
		}
	}
}
