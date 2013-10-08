using UnityEngine;
using System.Collections;

public class SettingsScreen : MonoBehaviour {
	public bool toggleGPS = true;
	public bool togglePaceDemo = false;
	public bool toggleStuff = false;
	public bool MenuOpen = false;
	public bool Runner = false;
	public bool Eagle = false;
	public bool train = false;
	public bool Zombie = false;
	private bool changed = false;
	public bool indoor = true;
	
	private Platform inputData = null;
	
	public GameObject eagleHolder;
	public GameObject RunnerHolder;
	public GameObject ZombieHolder;
	public GameObject trainHolder;
	
	private int originalHeight = 500;
	private int originalWidth = 800;
	private Vector3 scale;
	
	private bool started = false;
	private bool countdown = false;
	private float countTime = 3.0f;
	public float targSpeed = 1.8f;
	public float hSliderValue1 = 0.0F;
	public float hSliderValue2 = 0.0F;
	public float hSliderValue3 = 0.0F;
	public Texture BlackTexture;
	
	private string indoorText = "Indoor Active";
	
	void OnGUI() {
		
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		GUI.skin.button.fontSize = 15;
		
		if(MenuOpen)
		{
			GUI.DrawTexture(new Rect(0,0,originalWidth,originalHeight), BlackTexture);
					
			if(GUI.Button(new Rect(0, originalHeight-200, 200, 200) , "Runner"))
			{
				Runner = true;
				Eagle = false;
				Zombie = false;
				train = false;
				
				changed = true;
				
				ZombieHolder.SetActive(Zombie);
				eagleHolder.SetActive(Eagle);
				RunnerHolder.SetActive(Runner);
				trainHolder.SetActive(train);
			}
			
			if(GUI.Button(new Rect (200, originalHeight-200, 200, 200), "Eagle"))
			{
				Runner = false;
				Eagle = true;
				Zombie = false;
				train = false;
				changed = true;
				
				ZombieHolder.SetActive(Zombie);
				eagleHolder.SetActive(Eagle);
				RunnerHolder.SetActive(Runner);
				trainHolder.SetActive(train);
			}
			
			if(GUI.Button(new Rect (400, originalHeight-200, 200, 200), "Zombie"))
			{
				Runner = false;
				Eagle = false;
				Zombie = true;
				train = false;
				changed = true;
				
				ZombieHolder.SetActive(Zombie);
				eagleHolder.SetActive(Eagle);
				RunnerHolder.SetActive(Runner);
				trainHolder.SetActive(train);
			}
			
			if(GUI.Button(new Rect(600, originalHeight-200, 200, 200), "Train"))
			{
				Runner = false;
				Eagle = false;
				Zombie = false;
				train = true;
				
				ZombieHolder.SetActive(Zombie);
				eagleHolder.SetActive(Eagle);
				RunnerHolder.SetActive(Runner);
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
    		if(temp != targSpeed)
			{
				changed = true;
				targSpeed = temp;
			}
	//	hSliderValue1 = GUI.HorizontalSlider(new Rect(((Screen.width)/2)-100, ((Screen.height)/2), 200, 30), hSliderValue1, 0.0F, 10.0F);
    
	//	hSliderValue2 = GUI.HorizontalSlider(new Rect(((Screen.width)/2)-100, ((Screen.height)/2)+50, 200, 30), hSliderValue2, 0.0F, 10.0F);
    
       // hSliderValue3 = GUI.HorizontalSlider(new Rect(((Screen.width)/2)-100, ((Screen.height)/2)+100, 200, 30), hSliderValue3, 0.0F, 10.0F);
    	
			if (GUI.Button(new Rect(10, ((originalHeight)/2)-50, 100, 50), "Back"))
			{
        	    MenuOpen = false;
				
				if(changed) {
					inputData.stopTrack();
					inputData.reset();
					inputData.setTargetSpeed(targSpeed);
					inputData.setIndoor(indoor);
					//inputData = new Platform();
					
					eagleHolder.GetComponent<EagleController>().indoor = indoor;
					RunnerHolder.GetComponent<PBRunnerController>().indoor = indoor;
					ZombieHolder.GetComponent<ZombieController>().indoor = indoor;
					trainHolder.GetComponent<TrainController>().indoor = indoor;
					
					eagleHolder.GetComponent<EagleController>().reset();
					RunnerHolder.GetComponent<PBRunnerController>().reset();
					ZombieHolder.GetComponent<ZombieController>().reset();
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
        		MenuOpen = true;
				
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
		
		if(Eagle)
		{
			eagleHolder.SetActive(true);
		}
		else if(Runner)
		{
			RunnerHolder.SetActive(true);
		}
		else if(Zombie)
		{
			ZombieHolder.SetActive(true);
		}
		else if(train)
		{
			trainHolder.SetActive(true);
		}
		
		eagleHolder.GetComponent<EagleController>().indoor = indoor;
		RunnerHolder.GetComponent<PBRunnerController>().indoor = indoor;
		ZombieHolder.GetComponent<ZombieController>().indoor = indoor;
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
