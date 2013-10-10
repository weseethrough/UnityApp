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
	
	// Debug
	private Rect debug;
	private const int MARGIN = 15;
	private bool authenticated = false;
	
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
	
		// Use this for initialization
	void Start () {
		inputData = new Platform();
		inputData.setIndoor(indoor);
		float x = (float)Screen.width/originalWidth;
		float y = (float)Screen.height/originalHeight;
		scale = new Vector3(x, y, 1);
		
		debug = new Rect((originalWidth-200), MARGIN, 200, 200);
		
		zombieHolder.SetActive(zombie);
		eagleHolder.SetActive(eagle);
		runnerHolder.SetActive(runner);
		trainHolder.SetActive(train);
	}
	
	void OnGUI() {
		
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		GUI.depth = 5;
		GUI.skin.button.fontSize = 15;
		GUI.skin.horizontalSliderThumb.fixedWidth = 60;
		GUI.skin.horizontalSliderThumb.fixedHeight = 60;
			
		if(menuOpen)
		{			
			GUI.DrawTexture(new Rect(0,0,originalWidth,originalHeight), blackTexture);
					
			GUI.skin.label.fontSize = 30;
			GUI.Label(new Rect(originalWidth/2-100, 10, 200, 40), "Speed Guide");
			
			GUI.skin.label.fontSize = 15;
			GUI.Label(new Rect(originalWidth/2 -100, 50, 200, 40), "Walking: 1.25m/s");
			GUI.Label (new Rect(originalWidth/2 - 100, 90, 200, 40), "Jogging: 2.2m/s");
			GUI.Label(new Rect(originalWidth/2 - 100, 130, 200, 40), "Running: 4.2m/s");
			GUI.Label(new Rect(originalWidth/2 - 100, 170, 200, 40), "Usain Bolt: 10.4m/s");
			
			if(GUI.Button(new Rect(0, originalHeight-200, 200, 200) , "Runner"))
			{
				runner = true;
				eagle = false;
				zombie = false;
				train = false;
				
				changed = true;
				
				GetComponent<TargetDisplay>().setOffset(0);
			}
			
			if(GUI.Button(new Rect (200, originalHeight-200, 200, 200), "Eagle"))
			{
				runner = false;
				eagle = true;
				zombie = false;
				train = false;
				changed = true;
				
				GetComponent<TargetDisplay>().setOffset(50);
			}
			
			if(GUI.Button(new Rect (400, originalHeight-200, 200, 200), "Zombie"))
			{
				runner = false;
				eagle = false;
				zombie = true;
				train = false;
				changed = true;
				
				GetComponent<TargetDisplay>().setOffset(20);
			}
			
			if(GUI.Button(new Rect(600, originalHeight-200, 200, 200), "Train"))
			{
				runner = false;
				eagle = false;
				zombie = false;
				train = true;
				
				changed = true;
				
				GetComponent<TargetDisplay>().setOffset(50);
			}
			
			
			if(GUI.Button(new Rect(0, 0, 100, 100), indoorText))
			{
				if(indoor) {
					indoor = false;
					UnityEngine.Debug.Log("Outdoor mode active");
					indoorText = "Outdoor Active";
				}
				else {
					indoor = true;
					UnityEngine.Debug.Log("Indoor mode active");
					indoorText = "Indoor Active";
				}
				changed = true;
			}
			
			float temp  = GUI.HorizontalSlider(new Rect((originalWidth/2)-100, 250, 200, 50), targSpeed,  1.25f, 10.4f);
    		GUI.Label(new Rect(originalWidth/2 + 120, 250, 100, 50), temp.ToString("f2") + "m/s");
			if(temp != targSpeed)
			{
				changed = true;
				targSpeed = temp;
			}
			
			if (GUI.Button(new Rect(10, ((originalHeight)/2)-50, 100, 50), "Back"))
			{
        	    menuOpen = false;
				
				if(changed) {
					//inputData.stopTrack();
					inputData.reset();
					inputData.setTargetSpeed(targSpeed);
					inputData.setIndoor(indoor);
					
					zombieHolder.SetActive(zombie);
					eagleHolder.SetActive(eagle);
					runnerHolder.SetActive(runner);
					trainHolder.SetActive(train);
					
					started = false;
					countdown = false;
					countTime = 3.0f;
					
					changed = false;
				} else {
					inputData.StartTrack(indoor);
				}
			}
			
			if (!authenticated && GUI.Button(debug, "Authenticate")) {
			inputData.authenticate();
			// TODO: check result
			authenticated = true;
			}
			if (authenticated && GUI.Button(debug, "Sync to server")) {
				inputData.syncToServer();
			}
		} 
		else
		{
			if (GUI.Button(new Rect(10, ((originalHeight)/2)-50, 100, 50), "Options")){
        		menuOpen = true;
				inputData.stopTrack();
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
