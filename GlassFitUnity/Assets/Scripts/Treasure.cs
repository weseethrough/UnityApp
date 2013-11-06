using UnityEngine;
using System.Collections;
using System;

public class Treasure : MonoBehaviour {

	private Position worldCoordinate;
	private Vector3 gameCoordinate;
	private bool obtained = false;
	private float treasureDist = 0.0f;
	
	private Rect distance;
	private Rect gpsLock;
	private string distanceText;	
	
	private float originalWidth = 800;  // define here the original resolution
  	private float originalHeight = 500; // you used to create the GUI contents 
 	private Vector3 scale;
	private const int MARGIN = 15;
	
	private const float OPACITY = 0.5f;
	
	private bool started = false;
	private bool countdown = false;
	private float countTime = 3.0f;
	
	Texture2D normal;
	
	// Use this for initialization
	void Start () {
		worldCoordinate = new Position(UnityEngine.Random.Range(51.5320f, 51.5380f), UnityEngine.Random.Range(0.1353f, 0.1453f));
		UnityEngine.Debug.Log("Chest position is: " + worldCoordinate.latitude + ", " + worldCoordinate.longitude);
		
		distance = new Rect((originalWidth/2.0f) - 100, MARGIN, 200, 100);
		distanceText = "Distance\n";
		
		Color white = new Color(0.9f, 0.9f, 0.9f, OPACITY);
		normal = new Texture2D(1, 1);
		normal.SetPixel(0,0,white);
		normal.Apply();
		
		gpsLock =  new Rect(MARGIN, MARGIN, 200, 100);
		
		scale.x = (float)Screen.width / originalWidth;
		scale.y = (float)Screen.height / originalHeight;
    	scale.z = 1;
	}
	
	void OnGUI ()
	{
		// Scale for devices
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		GUI.depth = 10;
		// Setting label font size
		GUI.skin.label.fontSize = 15;
		
		// Setting GUI box attributes
		GUI.skin.box.wordWrap = true;
		GUI.skin.box.fontSize = 30;
		GUI.skin.box.fontStyle = FontStyle.Bold;
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;	
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUI.skin.box.normal.background = normal;
		GUI.skin.box.normal.textColor = Color.black;
		
		GUI.Box(distance, distanceText + treasureDist);
		
		if(!Platform.Instance.hasLock())
		{
			GUI.Label(gpsLock, "Waiting for GPS Lock...");
		}
		
		if(countdown)
		{
			GUI.skin.label.fontSize = 40;
			
			// Get the current time rounded up
			int cur = Mathf.CeilToInt(countTime);
			
			// Display countdown on screen
			if(countTime > 0.0f)
			{
				GUI.Label(new Rect(300, 150, 200, 200), cur.ToString()); 
			}
			else if(countTime > -1.0f && countTime < 0.0f)
			{
				GUI.Label(new Rect(300, 150, 200, 200), "GO!"); 
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		Platform.Instance.Poll();
		
		if(Platform.Instance.hasLock()) {
			// Initiate the countdown
			countdown = true;
		 	if(countTime <= -1.0f && !started)
			{
				Platform.Instance.StartTrack();
				UnityEngine.Debug.LogWarning("Tracking Started");
				started = true;
			}
			else if(countTime > -1.0f)
			{
				UnityEngine.Debug.LogWarning("Counting Down");
				countTime -= Time.deltaTime;
			}
			UnityEngine.Debug.Log("Current Position is: " + Platform.Instance.Position().latitude + ", " + Platform.Instance.Position().longitude);
			
			Vector2 currentPos = mercatorToPixel(worldCoordinate) - mercatorToPixel(Platform.Instance.Position());
			
			
			gameCoordinate = new Vector3(currentPos.x, 0, currentPos.y);
			
			treasureDist = currentPos.magnitude;
			
			if(treasureDist < 10 && !obtained) {
				obtained = true;
				GetComponent<MeshRenderer>().enabled = false;
			}		
			
			transform.position  = gameCoordinate;
		} 
		
		if(!started) {
			transform.position = new Vector3(0.0f, 0.0f, 10000.0f);
		}
	}
	
	public bool isObtained() {
		return obtained;
	}
	
	Vector2 mercatorToPixel(Position mercator) {
		// Per google maps spec: pixelCoordinate = worldCoordinate * 2^zoomLevel
		//int scale = (int)Math.Pow(2, mapZoom);
		
		// Mercator to google world cooordinates
		Vector2 world = new Vector2(
			(float)(mercator.longitude+180)/360*256,
			(float)(
				(1 - Math.Log(
						Math.Tan(mercator.latitude * Math.PI / 180) +  
						1 / Math.Cos(mercator.latitude * Math.PI / 180)
					) / Math.PI
				) / 2
			) * 256
		);
//		Debug.Log(mercator.latitude + "," + mercator.longitude + " => " + world.x + "," + world.y);
		
		return world * 100000;
	}
	
//	private double gps2m(float lat_a, float lng_a, float lat_b, float lng_b) {
//	    float pk = (float) (180/3.14169);
//	
//	    float a1 = lat_a / pk;
//	    float a2 = lng_a / pk;
//	    float b1 = lat_b / pk;
//	    float b2 = lng_b / pk;
//	
//	    float t1 = Math.cos(a1) * Math.cos(a2) * Math.cos(b1) * Math.cos(b2);
//	    float t2 = Math.cos(a1) * Math.sin(a2) * Math.cos(b1) * Math.sin(b2);
//	    float t3 = Math.sin(a1) * Math.sin(b1);
//	    double tt = Math.acos(t1 + t2 + t3);
//	
//	    return 6366000*tt;
//	}
}
