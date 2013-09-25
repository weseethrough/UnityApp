using UnityEngine;
using System.Collections;

public class distanceMarker : MonoBehaviour {
	
#if UNITY_ANDROID && !UNITY_EDITOR
	private Platform inputData = null;
#else
	private PlatformDummy inputData = null;
#endif
	
	private int target = 50;
	
	//GameObjects for each marker
	public GameObject fiftyMarker;
	public GameObject fiftyMarker2;
	
	private bool started = false;

	// Boxes to check distance
	private double distance;
	private Rect distanceBox;
	
	void Start () 
	{	
	#if UNITY_ANDROID && !UNITY_EDITOR 
		inputData = new Platform();
	#else
		inputData = new PlatformDummy();
	#endif
		
		distanceBox = new Rect(Screen.width/2, Screen.height - 50, 50, 50);
		//inputData.Start(true);
	}
	
	void ResetMarkers()
	{
		fiftyMarker.transform.position = new Vector3(0, 0, 5000);
		fiftyMarker2.transform.position = new Vector3(0, 0, 5000);
	}
	
	void Update () 
	{
		inputData.Poll();
		distance = inputData.Distance();
		
		ResetMarkers();
		
		if(!started && Input.touchCount == 3)
		{
			started = true;
			inputData.Start(false);
		}
		
		// 50m markers
		if(distance > target - 20 && distance < target + 20)
		{
			double deltDist = target - distance;
			deltDist *= 6.666f;
			fiftyMarker.transform.position = new Vector3(15, 0, (float)deltDist);
			fiftyMarker2.transform.position = new Vector3(-15, 0, (float)deltDist);
		}
		
		if(distance > target + 20) 
		{
			target +=50;	
		}
	}
}
