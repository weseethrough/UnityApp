using UnityEngine;
using System.Collections;

public class distanceMarker : MonoBehaviour {
	
#if UNITY_ANDROID && !UNITY_EDITOR
	private Platform inputData = null;
#else
	private PlatformDummy inputData = null;
#endif
	
	//GameObjects for each marker
	public GameObject fiftyMarker;
	public GameObject fiftyMarker2;
	
	public GameObject hundredMarker;
	public GameObject hundredMarker2;
	
	public GameObject hundredFiftyMarker;
	public GameObject hundredFiftyMarker2;
	
	public GameObject twoHundredMarker;
	public GameObject twoHundredMarker2;
	
	public GameObject oneKilometerMarker;
	public GameObject oneKilometerMarker2;
	
	public GameObject twoKilometerMarker;
	public GameObject twoKilometerMarker2;
	
	public GameObject threeKilometerMarker;
	public GameObject threeKilometerMarker2;
	
	public GameObject fourKilometerMarker;
	public GameObject fourKilometerMarker2;
	
	public GameObject fiveKilometerMarker;
	public GameObject fiveKilometerMarker2;
	
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
		hundredMarker.transform.position = new Vector3(0, 0, 5000);
		hundredMarker2.transform.position = new Vector3(0, 0, 5000);
		hundredFiftyMarker.transform.position = new Vector3(0, 0, 5000);
		hundredFiftyMarker2.transform.position = new Vector3(0, 0, 5000);
		twoHundredMarker.transform.position = new Vector3(0, 0, 5000);
		twoHundredMarker2.transform.position = new Vector3(0, 0, 5000);
		oneKilometerMarker.transform.position = new Vector3(0, 0, 5000);
		oneKilometerMarker2.transform.position = new Vector3(0, 0, 5000);
		twoKilometerMarker.transform.position = new Vector3(0, 0, 5000);
		twoKilometerMarker2.transform.position = new Vector3(0, 0, 5000);
		threeKilometerMarker.transform.position = new Vector3(0, 0, 5000);
		threeKilometerMarker2.transform.position = new Vector3(0, 0, 5000);
		fourKilometerMarker.transform.position = new Vector3(0, 0, 5000);
		fourKilometerMarker2.transform.position = new Vector3(0, 0, 5000);
		fiveKilometerMarker.transform.position = new Vector3(0, 0, 5000);
		fiveKilometerMarker2.transform.position = new Vector3(0, 0, 5000);
	}
	
	void Update () 
	{
		inputData.Poll();
		distance = 50;
		
		ResetMarkers();
		
		// 50m markers
		if(distance > 20 && distance < 80)
		{
			float deltDist = 50 - distance;
			deltDist *= 6.666f;
			fiftyMarker.transform.position = new Vector3(15, 0, deltDist);
			fiftyMarker2.transform.position = new Vector3(-15, 0, deltDist);
		}
		
		// 100m markers
		if(distance > 70 && distance < 130)
		{
			float deltDist = 100 - distance;
			deltDist *= 6.666f;
			hundredMarker.transform.position = new Vector3(15, -30, deltDist);
			hundredMarker2.transform.position = new Vector3(-15, -30, deltDist);
		}
		
		// 150m markers
		if(distance > 130 && distance < 180)
		{
			float deltDist = 150 - distance;
			deltDist *= 6.666f;
			hundredFiftyMarker.transform.position = new Vector3(15, -30, deltDist);
			hundredFiftyMarker2.transform.position = new Vector3(-15, -30, deltDist);
		}
		
		// 200m markers
		if(distance > 170 && distance < 230)
		{
			float deltDist = 200 - distance;
			deltDist *= 6.666f;
			twoHundredMarker.transform.position = new Vector3(15, -30, deltDist);
			twoHundredMarker2.transform.position = new Vector3(-15, -30, deltDist);
		}
		
		// 1km markers
		if(distance > 970 && distance < 1030)
		{
			float deltDist = 1000 - distance;
			deltDist *= 6.666f;
			oneKilometerMarker.transform.position = new Vector3(15, -30, deltDist);
			oneKilometerMarker2.transform.position = new Vector3(-15, -30, deltDist);
		}
		
		// 2km markers
		if(distance > 1970 && distance < 2030)
		{
			float deltDist = 2000 - distance;
			deltDist *= 6.666f;
			twoKilometerMarker.transform.position = new Vector3(15, -30, deltDist);
			twoKilometerMarker2.transform.position = new Vector3(-15, -30, deltDist);
		}
		
		// 3km markers
		if(distance > 2970 && distance < 3030)
		{
			float deltDist = 3000 - distance;
			deltDist *= 6.666f;
			threeKilometerMarker.transform.position = new Vector3(15, -30, deltDist);
			threeKilometerMarker2.transform.position = new Vector3(-15, -30, deltDist);
		}
		
		// 4km markers
		if(distance > 3970 && distance < 4030)
		{
			float deltDist = 4000 - distance;
			deltDist *= 6.666f;
			fourKilometerMarker.transform.position = new Vector3(15, -30, deltDist);
			fourKilometerMarker2.transform.position = new Vector3(-15, -30, deltDist);
		}
		
		// 5km markers
		if(distance > 4970 && distance < 5030)
		{
			float deltDist = 5000 - distance;
			deltDist *= 6.666f;
			fiveKilometerMarker.transform.position = new Vector3(15, -30, deltDist);
			fiveKilometerMarker2.transform.position = new Vector3(-15, -30, deltDist);
		}
	}
}
