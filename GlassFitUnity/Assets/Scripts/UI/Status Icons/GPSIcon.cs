using UnityEngine;
using System.Collections;

public class GPSIcon : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Platform.Instance.HasGpsProvider())
		{
			if(!renderer.enabled)
			{
				renderer.enabled = true;
			}
			if(Application.loadedLevelName == "Start Hex" || Application.loadedLevelName == "Game End")
			{
				renderer.material.color = Color.black;
			}
			else
			{
				renderer.material.color = Color.white;
			}
		}
		else
		{
			if(renderer.enabled)
			{
				renderer.enabled = false;
			}
		}
	}
}
