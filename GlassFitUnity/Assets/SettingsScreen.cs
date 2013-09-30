using UnityEngine;
using System.Collections;

public class SettingsScreen : MonoBehaviour {
	public bool toggleGPS = true;
	public bool togglePaceDemo = false;
	public bool toggleStuff = false;
	public bool MenuOpen = false;
	public bool Runner = false;
	public bool Train = false;
	public bool Zombie = false;
	public GameObject TrainHolder;
	
	public GameObject RunnerHolder;
	
	public GameObject ZombieHolder;
	
	public float hSliderValue = 0.0F;
	public float hSliderValue1 = 0.0F;
	public float hSliderValue2 = 0.0F;
	public float hSliderValue3 = 0.0F;
	public Texture BlackTexture;
    void OnGUI() {
		
		
		
		if(MenuOpen)
		{
			
			GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), BlackTexture);
		
			
			if(GUI.Button(new Rect(250, 105, 100, 30) , "Runner"))
			{
				Runner = true;
				Train = false;
				Zombie = false;
				ZombieHolder.SetActive(Zombie);
				TrainHolder.SetActive(Train);
				RunnerHolder.SetActive(Runner);
			
			}
			
			if(GUI.Button(new Rect (250, 145, 100, 30), "Train"))
			{
				Runner = false;
				Train = true;
				Zombie = false;
				ZombieHolder.SetActive(Zombie);
				TrainHolder.SetActive(Train);
				RunnerHolder.SetActive(Runner);
			
			}
			
			if(GUI.Button(new Rect (250, 185, 100, 30), "Zombie"))
			{
				Runner = false;
				Train = false;
				Zombie = true;
				ZombieHolder.SetActive(Zombie);
				TrainHolder.SetActive(Train);
				RunnerHolder.SetActive(Runner);
			
			}
			
			
				
			
			/*/
		//if(toggleGPS){
			toggleGPS = GUI.Toggle(new Rect (250, 25, 100, 30), toggleGPS, "GPS On");
		//}else{
	//		toggleGPS = GUI.Toggle (new Rect (250, 25, 100, 30), toggleGPS, "GPS Off");
	//	}
		
		if(togglePaceDemo){
			togglePaceDemo = GUI.Toggle (new Rect (250, 65, 100, 30), togglePaceDemo, "Demo Mode On");
		}else{
			toggleGPS = 	 GUI.Toggle (new Rect (250, 65, 100, 30), togglePaceDemo, "Demo Mode Off");
		}
		
		if(toggleStuff){
			toggleStuff = 	GUI.Toggle (new Rect (250, 105, 100, 30), toggleStuff, "Stuff is On");
		}else{
			toggleStuff = 	GUI.Toggle (new Rect (250, 105, 100, 30), toggleStuff, "Stuff is Off");
		}
    //*/// Check if the toggle was toggled

		
	//	hSliderValue  = GUI.HorizontalSlider(new Rect(((Screen.width)/2)-100, ((Screen.height)/2) - 50, 200, 20), hSliderValue,  0.0F, 10.0F);
    
	//	hSliderValue1 = GUI.HorizontalSlider(new Rect(((Screen.width)/2)-100, ((Screen.height)/2), 200, 30), hSliderValue1, 0.0F, 10.0F);
    
	//	hSliderValue2 = GUI.HorizontalSlider(new Rect(((Screen.width)/2)-100, ((Screen.height)/2)+50, 200, 30), hSliderValue2, 0.0F, 10.0F);
    
       // hSliderValue3 = GUI.HorizontalSlider(new Rect(((Screen.width)/2)-100, ((Screen.height)/2)+100, 200, 30), hSliderValue3, 0.0F, 10.0F);
    	
		if (GUI.Button(new Rect(10, ((Screen.height)/2)-50, 100, 50), "Back"))
            MenuOpen = false;
		
		}
	
		
		
		else
		{
			if (GUI.Button(new Rect(10, ((Screen.height)/2)-50, 100, 50), "Options")){
            MenuOpen = true;
			}
		}
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		
	}
}
