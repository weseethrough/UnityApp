using UnityEngine;
using System.Collections;

public class SettingsScreen : MonoBehaviour {
	public bool toggleGPS = true;
	public bool togglePaceDemo = false;
	public bool toggleStuff = false;
	public bool MenuOpen = false;
	public bool Runner = true;
	public bool Train = false;
	public bool Zombie = false;
	private bool changed = false;
	private bool indoor = true;
	
	private Platform inputData = null;
	public GameObject TrainObject;
	public GameObject RunnerObject;
	public GameObject TrainHolder;
	public GameObject RunnerHolder;
	public GameObject ZombieHolder;
	
	private int originalHeight = 500;
	private int originalWidth = 800;
	private Vector3 scale;
	
	private bool started = false;
	private bool countdown = false;
	private float countTime = 3.0f;
	public float hSliderValue = 0.0F;
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
				Train = false;
				Zombie = false;
				changed = true;
				//ZombieHolder.SetActive(Zombie);
				
				TrainHolder.SetActive(Train);
				RunnerHolder.SetActive(Runner);
				inputData.reset();
				Debug.Log("Train is: " + TrainHolder.activeSelf);
			}
			
			if(GUI.Button(new Rect (300, originalHeight-200, 200, 200), "Train"))
			{
				Runner = false;
				Train = true;
				Zombie = false;
				changed = true;
				//ZombieHolder.SetActive(Zombie);
				
				TrainHolder.SetActive(Train);
				RunnerHolder.SetActive(Runner);
				inputData.reset();
				Debug.Log("Train is: " + TrainHolder.activeSelf);
			}
			
			if(GUI.Button(new Rect (600, originalHeight-200, 200, 200), "Zombie"))
			{
				Runner = false;
				Train = false;
				Zombie = true;
				changed = true;
				//ZombieHolder.SetActive(Zombie);
				TrainHolder.SetActive(Train);
				RunnerHolder.SetActive(Runner);
			}
			
			
			if(GUI.Button(new Rect(0, 0, 100, 100), indoorText))
			{
				if(indoor) {
					indoor = false;
					UnityEngine.Debug.Log("Outdoor mode active");
					indoorText = "Outdoor Active";
					TrainHolder.GetComponent<TrainController>().indoor = false;
					RunnerHolder.GetComponent<PBRunnerController>().indoor = false;
					changed = true;
					inputData = null;
					inputData = new Platform();
				}
				else {
					indoor = true;
					UnityEngine.Debug.Log("Indoor mode active");
					indoorText = "Indoor Active";
					TrainHolder.GetComponent<TrainController>().indoor = true;
					RunnerHolder.GetComponent<PBRunnerController>().indoor = true;
					changed = true;
					inputData = null;
					inputData = new Platform();
				}
			}
			
	//	hSliderValue  = GUI.HorizontalSlider(new Rect(((Screen.width)/2)-100, ((Screen.height)/2) - 50, 200, 20), hSliderValue,  0.0F, 10.0F);
    
	//	hSliderValue1 = GUI.HorizontalSlider(new Rect(((Screen.width)/2)-100, ((Screen.height)/2), 200, 30), hSliderValue1, 0.0F, 10.0F);
    
	//	hSliderValue2 = GUI.HorizontalSlider(new Rect(((Screen.width)/2)-100, ((Screen.height)/2)+50, 200, 30), hSliderValue2, 0.0F, 10.0F);
    
       // hSliderValue3 = GUI.HorizontalSlider(new Rect(((Screen.width)/2)-100, ((Screen.height)/2)+100, 200, 30), hSliderValue3, 0.0F, 10.0F);
    	
			if (GUI.Button(new Rect(10, ((originalHeight)/2)-50, 100, 50), "Back"))
			{
        	    MenuOpen = false;
				if(changed) {
					inputData.reset();
					TrainHolder.GetComponent<TrainController>().reset();
					RunnerHolder.GetComponent<PBRunnerController>().reset();
					started = false;
					countdown = false;
					countTime = 3.0f;
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
				GUI.Label(new Rect(300, 200, 200, 200), cur.ToString()); 
			}
			else if(countTime > -1.0f && countTime < 0.0f)
			{
				GUI.Label(new Rect(300, 200, 200, 200), "GO!"); 
			}
		}
	}
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
		float x = (float)Screen.width/originalWidth;
		float y = (float)Screen.height/originalHeight;
		scale = new Vector3(x, y, 1);
	}
	
	// Update is called once per frame
	void Update () {
		
		if(countTime == 3.0f && inputData.hasLock() && !countdown)
		{
			//started = true;
			countdown = true;
		}
		else if(countTime <= -1.0f && !started)
		{
			inputData.StartTrack(indoor);
			started = true;
		}
		else if(countdown && countTime > -1.0f)
		{
			countTime -= Time.deltaTime;
		}
	}
}
