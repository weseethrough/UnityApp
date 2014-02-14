using UnityEngine;
using System.Collections;

public class MiniGameRun : GameBase {

	// Use this for initialization
	void Start () {
		base.Start();
		StartCoroutine( ScheduleRock() );
	}
	
	IEnumerator ScheduleRock()
	{
		yield return new WaitForSeconds(10.0f);
		
		//load the scene with the rock
		Application.LoadLevelAdditiveAsync("RockTest");
		
		//unload the scene with the rock
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
