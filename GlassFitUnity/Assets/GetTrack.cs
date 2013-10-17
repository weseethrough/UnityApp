using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class GetTrack : MonoBehaviour {
	
	private Platform inputData;
	private float originalWidth = 800;
	private float originalHeight = 500;
	//float mapZoom = 12;
	public static Texture2D lineTex;
	private Vector3 scale;
	private List<Position> curTrackPositions;
	Texture2D normal;
	private bool started = false;
	
	private Rect map;
	private const int MAP_RADIUS = 100;
	private const int MARGIN = 15;
	private const float OPACITY = 0.5f;
	Texture2D mapTexture = null;
	Material mapStencil;
	const int mapAtlasRadius = 315; // API max width/height is 640
	int mapZoom = 18;
	Position mapOrigo = new Position(0, 0);
	WWW mapWWW = null;
	Position fetchOrigo = new Position(0, 0);
	private bool mapChanged = true;
	
	private float latHigh;
	private float latLow;
	private float longHigh;
	private float longLow;
	private float centerLat;
	private float centerLong;
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
		//inputData.getTracks();
		//curTrackPositions = inputData.getTrackPositions();
		
		scale.x = Screen.width/originalWidth;
		scale.y = Screen.height/originalHeight;
		scale.z = 1;
		
		Color white = new Color(0.9f, 0.9f, 0.9f, 0.5f);
		normal = new Texture2D(1, 1);
		normal.SetPixel(0,0,white);
		normal.Apply();
		
		map =      new Rect(100, 50, 600, 400);
		mapStencil = new Material(Shader.Find("Custom/MapStencil"));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnGUI() {
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		GUI.depth = 10;
		GUI.skin.box.wordWrap = true;
		GUI.skin.box.fontSize = 30;
		GUI.skin.box.fontStyle = FontStyle.Bold;
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;				
		GUI.skin.box.normal.background = normal;
		GUI.skin.box.normal.textColor = Color.black;
		
		if(GUI.Button(new Rect(0, originalHeight/2, 100, 100), "Previous Track")) {
			inputData.getPreviousTrack();
			curTrackPositions = inputData.getTrackPositions();
			GetLimits();
			mapChanged = true;
			mapTexture = null;
		}
		
		if(GUI.Button(new Rect(originalWidth-100, originalHeight/2, 100, 100), "Next Track")) {
			inputData.getNextTrack();
			curTrackPositions = inputData.getTrackPositions();
			GetLimits();
			mapChanged = true;
			mapTexture = null;
		}
		
		if(GUI.Button(new Rect(originalWidth-100, originalHeight-100, 100, 100), "Get Tracks")) {
			inputData.getTracks();
			curTrackPositions = inputData.getTrackPositions();
			started = true;
			mapChanged = true;
			mapTexture = null;
			GetLimits();			
		}
		
		if(started) {
		
			double sin = Math.Sin(latHigh * Math.PI / 180);
			double radX2 = Math.Log((1 + sin) / (1-sin)) / 2;
			double neLat = Math.Max(Math.Min(radX2, Math.PI), -Math.PI) /2;
		
			sin = Math.Sin(latLow * Math.PI / 180);
			radX2 = Math.Log((1 + sin) / (1-sin)) / 2;
			double swLat = Math.Max(Math.Min(radX2, Math.PI), -Math.PI) / 2;
		
			double latFraction = (neLat - swLat) / Math.PI;
		
			double longDiff = longHigh - longLow;
			double longFraction = ((longDiff < 0) ? (longDiff + 360) : longDiff) / 360;
		
			double latZoom = Math.Floor (Math.Log(400 / 256 / latFraction) / 0.6931471805599453 );
			double longZoom = Math.Floor(Math.Log(600 / 256 / longFraction) / 0.6931471805599453);
		
			mapZoom = (int)Math.Min(latZoom, longZoom);
				if(mapZoom > 21) {
					mapZoom = 21;
				}
		
		//		if(started)
//			GUI.Label(new Rect(originalWidth/2 - 100, originalHeight/2, 200, 100), "Number of Positions in track: " + curTrackPositions.Count.ToString());
		
			//if(mapTexture == null) {
				if ((mapWWW == null && mapTexture == null) && mapChanged){
					const string API_KEY = "AIzaSyBj_iHOwteDxJ8Rj_bPsoslxIquy--y9nI";
					const string endpoint = "http://maps.googleapis.com/maps/api/staticmap";
					string url = endpoint + "?center="
			 	                      	  + centerLat.ToString() + "," + centerLong.ToString()
			  	               	      	  + "&zoom=" + mapZoom.ToString()
			    	                  	  + "&size=" + "600" + "x" + "400"
			        	              	  + "&maptype=roadmap"
			            	          	  + "&sensor=true&key=" + API_KEY;
					mapWWW = new WWW(url);
					UnityEngine.Debug.Log("Map: URL is: " + url);
					}
					if (mapWWW != null && mapWWW.isDone) {
						if (mapWWW.error != null) {
						//debugText = mapWWW.error;
						UnityEngine.Debug.LogWarning(mapWWW.error);
						UnityEngine.Debug.LogWarning("Map: error with map");
					} else {
						//debugText = "";
						mapTexture = mapWWW.texture;
						mapChanged = false;
						//mapOrigo = fetchOrigo;
						UnityEngine.Debug.Log("Map: origo error");
					}
					mapWWW = null;
				}
						
				if (mapTexture == null) {
					GUI.Label(map, "Fetching map...    zoomLevel = " + mapZoom.ToString());
				}
		//		
					// Rotation and indexing into atlas handled by shader
					//mapStencil.SetFloat("_Rotation", (float)-bearing);			
					//mapStencil.SetVector("_Rectangle", new Vector4(map.x, map.y, map.width, map.height));
		//			Graphics.DrawTexture(map, mapTexture, mapCoords, 0, 0, 0, 0, mapStencil);
					GUI.DrawTexture(map, mapTexture);
				
			//}
		}
	}
	
	
	
	void GetLimits() {
		latLow = longLow = 360;
		latHigh = longHigh = 0;
		float totalLat = 0;
		float totalLong = 0;
		for(int i=0; i<curTrackPositions.Count; i++)
		{
//			if(curTrackPositions[i].latitude < latLow) {
//				latLow = curTrackPositions[i].latitude;
//			}
//			if(curTrackPositions[i].latitude > latHigh) {
//				latHigh = curTrackPositions[i].latitude;
//			}
//			if(curTrackPositions[i].longitude < longLow) {
//				longLow = curTrackPositions[i].longitude;
//			}
//			if(curTrackPositions[i].longitude > longHigh) {
//				longHigh = curTrackPositions[i].longitude;
//			}
			totalLat += curTrackPositions[i].latitude;
			totalLong += curTrackPositions[i].longitude;
		}
		centerLat = totalLat / curTrackPositions.Count;
		centerLong = totalLong / curTrackPositions.Count;
//		float distLat = latHigh - latLow;
//		float distLong = longHigh - longLow;
//		centerLat = latHigh - (distLat / 2.0f);
//		centerLong = longHigh - (distLong / 2.0f);
	}
	
	Vector2 mercatorToPixel(Position mercator) {
		// Per google maps spec: pixelCoordinate = worldCoordinate * 2^zoomLevel
		int scale = (int)Math.Pow(2, mapZoom);
		
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
		
		return world * scale;
	}
	
	Position pixelToMercator(Vector2 pixel) {
		// Per google maps spec: pixelCoordinate = worldCoordinate * 2^zoomLevel
		int scale = (int)Math.Pow(2, mapZoom);
		
		Vector2 world = pixel / scale;
		// Google world coordinates to mercator
		double n = Math.PI - 2 * Math.PI * world.y / 256;
		Position mercator = new Position(
			(float)(180 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)))),
			(float)world.x / 256 * 360 - 180
		);
			
		return mercator;			
	}
	
	private double Angle(Vector2 pos1, Vector2 pos2) {
	    Vector2 from = pos2 - pos1;
	    Vector2 to = new Vector2(0, 1);
	 
	    float result = Vector2.Angle( from, to );
//		Debug.Log(result);
	    Vector3 cross = Vector3.Cross( from, to );
	 
	    if (cross.z > 0)
	       result = 360f - result;
		
		result += 180.0f;
		
	    return result*Math.PI/180;
	}
	
	public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width) {
		Matrix4x4 matrix = GUI.matrix;
 
        // Generate a single pixel texture if it doesn't exist
        if (!lineTex) { lineTex = new Texture2D(1, 1); }
 
        // Store current GUI color, so we can switch it back later,
        // and set the GUI color to the color parameter
        Color savedColor = GUI.color;
        GUI.color = color;
 
        // Determine the angle of the line.
        float angle = Vector3.Angle(pointB - pointA, Vector2.right);
 
        // Vector3.Angle always returns a positive number.
        // If pointB is above pointA, then angle needs to be negative.
        if (pointA.y > pointB.y) { angle = -angle; }
 
        // Use ScaleAroundPivot to adjust the size of the line.
        // We could do this when we draw the texture, but by scaling it here we can use
        //  non-integer values for the width and length (such as sub 1 pixel widths).
        // Note that the pivot point is at +.5 from pointA.y, this is so that the width of the line
        //  is centered on the origin at pointA.
        GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));
 
        // Set the rotation for the line.
        //  The angle was calculated with pointA as the origin.
        GUIUtility.RotateAroundPivot(angle, pointA);
 
        // Finally, draw the actual line.
        // We're really only drawing a 1x1 texture from pointA.
        // The matrix operations done with ScaleAroundPivot and RotateAroundPivot will make this
        //  render with the proper width, length, and angle.
        GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), lineTex);
 
        // We're done.  Restore the GUI matrix and GUI color to whatever they were before.
        GUI.matrix = matrix;
        GUI.color = savedColor;
	}
}
