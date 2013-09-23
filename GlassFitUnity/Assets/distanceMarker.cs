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

	// Boxes to check distance
	private long distance = 0;
	private Rect distanceBox;
	
	void Start () 
	{	
	#if UNITY_ANDROID && !UNITY_EDITOR 
		inputData = new Platform();
	#else
		inputData = new PlatformDummy();
	#endif
		
		distanceBox = new Rect(Screen.width/2, Screen.height - 50, 50, 50);
		inputData.Start(true);
	}
	
	void OnGUI()
	{
		GUI.skin.box.fontSize = 30;
		GUI.Box(distanceBox, distance.ToString());
		GUI.Box(new Rect(0, 0, 300, 50), fiftyMarker.transform.position.ToString());
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
		
		// 50m markers
		if(distance > target - 20 && distance < target + 20)
		{
			float deltDist = target - distance;
			deltDist *= 6.666f;
			fiftyMarker.transform.position = new Vector3(15, 0, deltDist);
			fiftyMarker2.transform.position = new Vector3(-15, 0, deltDist);
		}
		
		if(distance > target + 20) 
		{
			target +=50;	
		}
	}
}
