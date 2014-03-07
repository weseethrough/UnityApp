using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// Mini map. Manages the minimap on the UI - updates its image and
/// </summary>
public class MiniMap : MonoBehaviour {
	
	// Minimap attributes
	private GameObject minimap;
	private const int MAP_RADIUS = 1;
	Texture2D selfIcon;
	Texture2D targetIcon;
	Texture2D mapTexture = null;
	Material mapStencil;
	const int mapAtlasRadius = 315; // API max width/height is 640
	const int mapZoom = 18;
	Position mapOrigo = new Position(0, 0);
	WWW mapWWW = null;
	Position fetchOrigo = new Position(0, 0);
	
	// Variables to set the scale
	private int originalHeight = 500;
	private int originalWidth = 800;
	private Vector3 scale;
	
	// Use this for initialization
	void Start () {
	
		//Get the minimap object. The script should actually be attached to this object anyway
		minimap = GameObject.Find("minimap");
		minimap.renderer.material.renderQueue = 3000;
		
		// Calculate and set scale
		float x = (float)Screen.width/originalWidth;
		float y = (float)Screen.height/originalHeight;
		scale = new Vector3(x, y, 1);
		
	}
	
	// Update is called once per frame
	/// <summary>
	/// Update this instance. Fetch distance/position/bearing and update map accordingly.
	/// </summary>
	void Update () {
	
				// TODO: Multiple minimap targets
		double targetDistance = Platform.Instance.GetHighestDistBehind();
		Position position = Platform.Instance.LocalPlayerPosition.Position;
		float bearing = Platform.Instance.LocalPlayerPosition.Bearing;

		double bearingRad = bearing*Math.PI/180;
		
		if (position != null) {
			// Fake target coord using distance and bearing
			Position targetCoord = new Position(position.latitude + (float)(Math.Cos(bearingRad)*targetDistance/111229d), position.longitude + (float)(Math.Sin(bearingRad)*targetDistance/111229d));
			GetMap(position, bearingRad, targetCoord);
		}
		
		
		
	}
	
	
	/// <summary>
	/// Retrieves the map and updates the texture on the HUD
	/// </summary>
	/// <param name='selfCoords'>
	/// Self coords.
	/// </param>
	/// <param name='bearing'>
	/// Bearing.
	/// </param>
	/// <param name='targetCoords'>
	/// Target coords.
	/// </param>
	private void GetMap(Position selfCoords, double bearing, Position targetCoords) {
		Position direction = new Position(selfCoords.latitude + (float)(Math.Cos(bearing)*1000/111229d), 
		                                  selfCoords.longitude + (float)(Math.Sin(bearing)*1000/111229d));	
		double pixelBearing = Angle(MercatorToPixel(selfCoords), MercatorToPixel(direction));	
//		UnityEngine.Debug.Log("Map: pixel bearing calculated");
		bearing = pixelBearing;
		
		// Get a static map with a radius of mapAtlasRadius, cache and re-get if viewport within margin of the border
		const int margin = 15;	
		int maxdrift = (mapAtlasRadius-MAP_RADIUS-margin);
		Vector2 drift = MercatorToPixel(mapOrigo) - MercatorToPixel(selfCoords);
//		UnityEngine.Debug.Log("Map: drift calculated");
//		Debug.Log("drift: " + drift.magnitude + " .." + drift);
		if (mapWWW == null && (mapTexture == null || drift.magnitude >= maxdrift)) {
			FetchMapTexture(selfCoords);
			UnityEngine.Debug.Log("Map: fetched");
		}
		if (mapWWW != null && mapWWW.isDone) {
			if (mapWWW.error != null) {
				UnityEngine.Debug.Log(mapWWW.error);
				UnityEngine.Debug.Log("Map: error with map");
			} else {
				mapTexture = mapWWW.texture;
				mapOrigo = fetchOrigo;
				UnityEngine.Debug.Log("Map: origo error");
			}
			mapWWW = null;
		}
		
		if (mapTexture == null) {
			return;
		}
		
		// Map self coordinates into map atlas, normalize to atlas size and shift to center
		Vector2 mapNormalSelf = (MercatorToPixel(mapOrigo) - MercatorToPixel(selfCoords)) / (mapAtlasRadius*2);
		mapNormalSelf.x += 0.5f;
		mapNormalSelf.y += 0.5f;
		float normalizedRadius = (float)MAP_RADIUS/(mapAtlasRadius*2);
		// Draw a MAP_RADIUS-sized circle around self
		Rect mapCoords = new Rect(1 - mapNormalSelf.x - normalizedRadius, mapNormalSelf.y - normalizedRadius,
		                          normalizedRadius*2, normalizedRadius*2);
		Vector2 mapCenter = new Vector2(minimap.transform.position.x, minimap.transform.position.y);
		//Matrix4x4 matrixBackup = GUI.matrix;
		if (Event.current.type == EventType.Repaint) {
			// Rotation and indexing into atlas handled by shader
			//minimap.renderer.material.SetFloat("_Rotation", (float)-bearing);			
			//minimap.renderer.material.SetVector("_Rectangle", new Vector4(mapCoords.x, mapCoords.y, mapCoords.width, mapCoords.height));
//			Graphics.DrawTexture(map, mapTexture, mapCoords, 0, 0, 0, 0, mapStencil);
			minimap.renderer.material.mainTexture = mapTexture;
//			minimap2.renderer.material.mainTexture = mapTexture;
		}
		
//		// Self is always at center
//		mapSelf.x = mapCenter.x - mapSelf.width/2;
//		mapSelf.y = mapCenter.y - mapSelf.height/2;
//		GUI.DrawTexture(mapSelf, selfIcon);
//		// Target is relative to self and limited to map radius
//		Vector2 localTarget = mercatorToPixel(selfCoords) - mercatorToPixel(targetCoords);
//		if (localTarget.magnitude > MAP_RADIUS) {
//			localTarget.Normalize();
//			localTarget *= MAP_RADIUS;
//			// TODO: Change icon to indicate outside of minimap?
//		}		
		
		// Rotated so bearing is up
		double c = Math.Cos(-bearing);
		double s = Math.Sin(-bearing);
//		mapTarget.x = mapCenter.x - (float)(localTarget.x * c - localTarget.y * s)  - mapTarget.width/2;
//		mapTarget.y = mapCenter.y - (float)(localTarget.x * s + localTarget.y * c) - mapTarget.height/2;
		//GUI.DrawTexture(mapTarget, targetIcon);
				
		//GUI.matrix = matrixBackup;
		//GUI.color = original;
	}
	
	Vector2 MercatorToPixel(Position mercator) {
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
	
	Position PixelToMercator(Vector2 pixel) {
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
	
	private void FetchMapTexture(Position origo) {					
		const string API_KEY = "AIzaSyBj_iHOwteDxJ8Rj_bPsoslxIquy--y9nI";
		const string endpoint = "http://maps.googleapis.com/maps/api/staticmap";
		string url = endpoint + "?center="
		                      + origo.latitude + "," + origo.longitude
//		                      + "&markers=color:blue%7Clabel:S%7C" + origo.latitude + "," + origo.longitude
//		                      + "&markers=color:green%7Clabel:E%7C" + (origo.latitude+0.0001f) + "," + (origo.longitude+0.0001f)
//		                      + "&markers=color:red%7Clabel:W%7C" + (origo.latitude) + "," + (origo.longitude+0.0003f)
		                      + "&zoom=" + mapZoom
		                      + "&size=" + (mapAtlasRadius*2) + "x" + (mapAtlasRadius*2)
		                      + "&maptype=roadmap"
		                      + "&sensor=true&key=" + API_KEY;
		mapWWW = new WWW(url);		
//		debugText = "Fetching map..";
		UnityEngine.Debug.Log("Fetching map.. " + url);
		fetchOrigo = origo;
	}

}
