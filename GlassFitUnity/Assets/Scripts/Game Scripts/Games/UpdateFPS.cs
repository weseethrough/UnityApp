using UnityEngine;
using System.Collections;

public class UpdateFPS : MonoBehaviour {
	
	float currentTime;
	public int fps;
    public int lastFPS;
	
	private Vector3 scale;
	
	// Use this for initialization
	void Start () 
	{
		currentTime = Time.timeSinceLevelLoad;
		scale.x = Screen.width/800f;
		scale.y = Screen.height/500f;
		scale.z = 1;
	}
	
	void OnGUI() {
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		GUIStyle labelStyle = new GUIStyle();
		labelStyle.fontSize = 40;
		
		labelStyle.normal.textColor = Color.white;
		GUI.Label(new Rect(300, 200, 200, 100), lastFPS.ToString(), labelStyle);
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
		//DataVault.Set("fps", lastFPS);
	}
}
