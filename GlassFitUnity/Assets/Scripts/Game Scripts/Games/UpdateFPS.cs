using UnityEngine;
using System.Collections;

public class UpdateFPS : MonoBehaviour {
	
	float currentTime;
	public int fps;
    public int lastFPS;
	
	// Use this for initialization
	void Start () 
	{
		currentTime = Time.timeSinceLevelLoad;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Time.timeSinceLevelLoad - currentTime <= 1.0f)
		{
			fps++;	
		}
		else
		{
			lastFPS = fps +1;
			currentTime = Time.timeSinceLevelLoad;
			fps = 0;
		}
		
		//update the database value
		DataVault.Set("fps", fps);
	}
}
