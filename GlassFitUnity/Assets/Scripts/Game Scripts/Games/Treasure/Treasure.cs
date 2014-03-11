using UnityEngine;
using System.Collections;
using System;

using RaceYourself.Models;

/// <summary>
/// Controls the position of the treasure and draws the GUI - needs updating
/// </summary>
public class Treasure : MonoBehaviour {
	
	// Real world position in lat and long.
	private Position worldCoordinate;
	
	// Game world position from lat and long.
	private Vector3 gameCoordinate;
	
	// Boolean for when treasure has been obtained
	private bool obtained = false;
	
	// Float for the distance to the treasure.
	private double treasureDist = 0.0;
	
	// Variables for starting the game.
	private bool started = false;
	private bool countdown = false;
	private float countTime = 3.0f;
	
	// OnGUI elements.
	// TODO: Change to nGUI.
	private Rect distance;
	private Rect gpsLock;
	private string distanceText;	
	
	// Scaling values for OnGUI - to be updated.
	private float originalWidth = 800; 
  	private float originalHeight = 500; 
 	private Vector3 scale;
	
	// Margin for space with OnGUI() - to be updated.
	private const int MARGIN = 15;
	
	// Opacity for boxes for OnGUI - to be updated.
	private const float OPACITY = 0.5f;
	
	// Texture for the boxes for OnGUI - to be updated.
	Texture2D normal;
	
	/// <summary>
	/// Start this instance. Sets the initial values
	/// </summary>
	void Start () {
		
		// Set real world position near Camden for testing purposes.
		// Final game will be based on user's position. 		
		worldCoordinate = new Position(UnityEngine.Random.Range(51.530479f, 51.539075f), UnityEngine.Random.Range(-0.142651f, -0.134411f));
		UnityEngine.Debug.Log("Chest position is: " + worldCoordinate.latitude + ", " + worldCoordinate.longitude);
	
		// Set the box to display distance.
		distance = new Rect((originalWidth/2.0f) - 100, MARGIN, 200, 100);
		distanceText = "Distance\n";
		
		// Set the colour of the box.
		Color white = new Color(0.9f, 0.9f, 0.9f, OPACITY);
		normal = new Texture2D(1, 1);
		normal.SetPixel(0,0,white);
		normal.Apply();
		
		// Set the box for the GPS Lock indicator.
		gpsLock =  new Rect(MARGIN, MARGIN, 200, 100);
		
		// Set the scale based on the size of the screen.
		scale.x = (float)Screen.width / originalWidth;
		scale.y = (float)Screen.height / originalHeight;
    	scale.z = 1;
		
		Platform.Instance.LocalPlayerPosition.SetIndoor(false);
	}
	
	void OnGUI()
	{
				// Scale for devices.
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		GUI.depth = 10;
		
		// Setting label font size.
		GUI.skin.label.fontSize = 15;
		
		// Setting GUI box attributes.
		GUI.skin.box.wordWrap = true;
		GUI.skin.box.fontSize = 30;
		GUI.skin.box.fontStyle = FontStyle.Bold;
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;	
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUI.skin.box.normal.background = normal;
		GUI.skin.box.normal.textColor = Color.black;
		
		// Add a box for the treasure distance.
		GUI.Box(distance, distanceText + treasureDist);
		
		// If there is no GPS lock, tell the player.
		if(!Platform.Instance.LocalPlayerPosition.HasLock())
		{
			GUI.Label(gpsLock, "Waiting for GPS Lock...");
		}
		
	}
	
	/// <summary>
	/// Update this instance. Updates the position.
	/// </summary>
	void Update () {
		
		Platform.Instance.Poll();
		
		if(Platform.Instance.LocalPlayerPosition.HasLock()) {
			
			// Initiate the countdown.
			if(!started)
			{
				Platform.Instance.LocalPlayerPosition.StartTrack();
				UnityEngine.Debug.LogWarning("Tracking Started");
				started = true;
			}
			
			UnityEngine.Debug.Log("Current Position is: " + Platform.Instance.LocalPlayerPosition.Position.latitude + ", " + Platform.Instance.LocalPlayerPosition.Position.longitude);
			
			// Get the current position of the treasure based on distance between the player and its real world position.
			//Vector2 currentPos = MercatorToPixel(worldCoordinate) - MercatorToPixel(Platform.Instance.LocalPlayerPosition.Position);
			
			// Get the magnitude of the distance.
			treasureDist = LatLongToMetre(Platform.Instance.LocalPlayerPosition.Position, worldCoordinate);
			
			gameCoordinate = new Vector3(0, 0, (float)treasureDist);
			
			// If the player is close enough, obtain the treasure.
			if(treasureDist < 5 && !obtained) {
				obtained = true;
				GetComponent<MeshRenderer>().enabled = false;
			}		
			
			double bearing = CalcBearing(Platform.Instance.Position(), worldCoordinate);
			
			UnityEngine.Debug.Log("Treasure: bearing is " + bearing.ToString("f2"));
			
			float yaw = Platform.Instance.GetPlayerOrientation().AsNorthReference(); // * (360 / (Mathf.PI * 2));
			
			UnityEngine.Debug.Log("Treasure: yaw is " + yaw.ToString("f2"));
			
			float finalYaw = (float)bearing - yaw;
			
			UnityEngine.Debug.Log("Treasure: final yaw is " + finalYaw.ToString("f2"));
			
			
			transform.root.rotation = Quaternion.Euler(new Vector3(0, (float)finalYaw, 0));
			
			// Set the position based on the game coordinate.
			transform.localPosition  = gameCoordinate;
		} 
		else
		{
			//UnityEngine.Debug.Log("Treasure: distance should actually be " + LatLongToMetre(new Position(51.535452f, -0.139732f), worldCoordinate).ToString("f2"));
			//CalcBearing(new Position(51.535452f, -0.139732f), worldCoordinate);
		}
		
		// If its not started, set the treasure's position far away out of sight.
		if(!started) {
			
			treasureDist = LatLongToMetre(new Position(51.535452f, -0.139732f), worldCoordinate);
			
			gameCoordinate = new Vector3(0, 0, (float)treasureDist);
			
			double bearing = CalcBearing(new Position(51.535452f, -0.139732f), worldCoordinate);
			
			UnityEngine.Debug.Log("Treasure: bearing is " + bearing.ToString("f2"));
			
			float yaw = Platform.Instance.GetPlayerOrientation().AsNorthReference(); //* (360 / (Mathf.PI * 2));
			
			UnityEngine.Debug.Log("Treasure: yaw is " + yaw.ToString("f2"));
			
			float finalYaw = (float)bearing - yaw;
			
			UnityEngine.Debug.Log("Treasure: final yaw is " + finalYaw.ToString("f2"));
			
			transform.root.rotation = Quaternion.Euler(new Vector3(0, finalYaw, 0));
			
			transform.localPosition = gameCoordinate;
		}
	}
	
	/// <summary>
	/// Returns true if the treasure has been obtained
	/// </summary>
	/// <returns>
	/// <c>true</c> if the treasure is obtained; otherwise, <c>false</c>.
	/// </returns>
	public bool IsObtained() {
		return obtained;
	}
	
	/// <summary>
	/// Converts a Mercator projection position to pixel position.
	/// </summary>
	/// <returns>
	/// The converted position.
	/// </returns>
	/// <param name='mercator'>
	/// Original position in Lat and Long
	/// </param>
	Vector2 MercatorToPixel(Position mercator) {
		
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
		return world * 100000;
	}
	
	private double LatLongToMetre(Position start, Position end)
	{
		float R = 6371000;
		
		float startLatInRads = start.latitude * ((Mathf.PI * 2) / 360f);
		float endLatInRads = end.latitude * ((Mathf.PI * 2) / 360f);
		float startLongInRads = start.longitude * ((Mathf.PI * 2) / 360f);
		float endLongInRads = end.longitude * ((Mathf.PI * 2) / 360f);
		
		double d = Math.Acos(Math.Sin(startLatInRads) * Math.Sin(endLatInRads) +
							 (Math.Cos(startLatInRads) * Math.Cos(endLatInRads) *
							 Math.Cos(endLongInRads - startLongInRads))) * R;
		return d;
	}
	
	private double CalcBearing(Position start, Position end)
	{
		float startLatInRads = start.latitude * ((Mathf.PI * 2) / 360f);
		float endLatInRads = end.latitude * ((Mathf.PI * 2) / 360f);
		float startLongInRads = start.longitude * ((Mathf.PI * 2) / 360f);
		float endLongInRads = end.longitude * ((Mathf.PI * 2) / 360f);
		
		double y = Math.Sin(endLongInRads - startLongInRads) * Math.Cos(endLatInRads);
		double x = Math.Cos(startLatInRads) * Math.Sin(endLatInRads) - 
				  Math.Sin(startLatInRads) * Math.Cos(endLatInRads) * Math.Cos(endLongInRads - startLongInRads);
		double bearing = Math.Atan2(y, x) * (360 / (Mathf.PI * 2));
		
		//UnityEngine.Debug.Log("Treasure: bearing without turning to 360 is " + bearing.ToString("f2"));
		
		bearing = (bearing + 360.0) % 360.0;
		
		//UnityEngine.Debug.Log("Treasure: bearing after turning to 360 is " + bearing.ToString("f2"));
		
		return bearing;
	}
}
