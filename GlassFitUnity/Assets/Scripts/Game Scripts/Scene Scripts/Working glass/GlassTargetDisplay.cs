using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Displays information about the target, working on Glass.
/// </summary>
public class GlassTargetDisplay: MonoBehaviour {

	// Constants for the radius, margin and box opacity.
	private const int MAP_RADIUS = 100;
	private const int MARGIN = 15;
	private const float OPACITY = 0.5f;
	
	// Rectangles for the OnGUI map.
	// TODO: update to nGUI.
	private Rect map;
	private Rect mapSelf;
	private Rect mapTarget;
	private Rect debug;
	private string debugText;
	
	// Scaling values for OnGUI.
	private Vector3 scale;
	private int originalHeight = 500;
	private int originalWidth = 800;
	
	// Rectangle for the target and text.
	private Rect target;	
	private string targetText;
	
	// Background textures
	Texture2D normal;
	Texture2D info;
	Texture2D warning;
	
	// Map textures
	Texture2D selfIcon;
	Texture2D targetIcon;
	Texture2D mapTexture = null;
	Material mapStencil;
	
	// API max width/height is 640
	const int mapAtlasRadius = 315; 
	const int mapZoom = 18;
	Position mapOrigo = new Position(0, 0);
	WWW mapWWW = null;
	Position fetchOrigo = new Position(0, 0);
	
	// Offset for the target.
	private float offset = 0;
	
	void Start () {
		// Set the scale.
		scale.x = (float)Screen.width / originalWidth;
		scale.y = (float)Screen.height / originalHeight;
    	scale.z = 1;
		
		// Initialise the Target box.
		target =   new Rect(15, 15, 200, 100);
		
		// Set the white colour for the regular boxes.
		Color white = new Color(0.9f, 0.9f, 0.9f, 0.5f);
		normal = new Texture2D(1, 1);
		normal.SetPixel(0,0,white);
		normal.Apply();
		
		// Set the green colour for when the user is ahead of the target.
		Color green = new Color(0f, 0.9f, 0f, 0.5f);
		info = new Texture2D(1, 1);
		info.SetPixel(0,0,green);
		info.Apply();
		
		// Set the red colour for when the user is behind the target.
		Color red = new Color(0.9f, 0f, 0f, 0.5f);
		warning = new Texture2D(1, 1);
		warning.SetPixel(0,0,red);
		warning.Apply();
		
		// Set the rectangles for the minimap and the objects displayed on the map.
		map =      new Rect(originalWidth-MARGIN-200, originalHeight-MARGIN-200, 200, 200);
		mapSelf =  new Rect(0, 0, 30, 30);
		mapTarget = new Rect(0, 0, 30, 30);
		
		// Load icons and stencil from resources.
		selfIcon = Resources.Load("Self") as Texture2D;
		targetIcon = Resources.Load("Target") as Texture2D;
		mapStencil = new Material(Shader.Find("Custom/MapStencil"));
	}
	
	void OnGUI() {
		// Initialise the GUI values.
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		GUI.depth = 10;
		GUI.skin.box.wordWrap = true;
		GUI.skin.box.fontSize = 30;
		GUI.skin.box.fontStyle = FontStyle.Bold;
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;				
		GUI.skin.box.normal.background = normal;
		GUI.skin.box.normal.textColor = Color.black;
		
		// Target Distance
		GUIStyle targetStyle = new GUIStyle(GUI.skin.box);
				
		// Get the distance behind the target.
		double targetDistance = Platform.Instance.GetHighestDistBehind() - offset;
		
		// Set the colour of the box and the text based on the distance.
		if (targetDistance > 0) {
			targetStyle.normal.background = warning; 
			targetText = "Behind!\n";
		} else {
			targetStyle.normal.background = info; 
			targetText = "Ahead\n";
		}
		
		// Set the box that displays the distance behind the target.
		targetStyle.normal.textColor = Color.white;		
		GUI.Box(target, targetText+"<i>"+SiDistance( Math.Abs(targetDistance) )+"</i>", targetStyle);
		
		// Get the current position in lat and long.
		Position position = Platform.Instance.Position();
		
		// Get the current bearing in degrees.
		float bearing = Platform.Instance.Bearing();
		
		// Convert the value to radians.
		double bearingRad = bearing*Math.PI/180;
		
		// If we have a position, get the minimap.
		if (position != null) {
			// Fake target coord using distance and bearing
			Position targetCoord = new Position(position.latitude + (float)(Math.Cos(bearingRad)*targetDistance/111229d), position.longitude + (float)(Math.Sin(bearingRad)*targetDistance/111229d));
			GUIMap(position, bearingRad, targetCoord);
		} else {
			GUI.Label(map, "No GPS lock");
		}
		GUI.matrix = Matrix4x4.identity;
	}
	
	/// <summary>
	/// Fetchs the map texture for the minimap using the Google Static Maps API.
	/// </summary>
	/// <param name='origo'>
	/// Origo is the current position of the player in lat and long that is used for the centre.
	/// </param>
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
		debugText = "Fetching map..";
		UnityEngine.Debug.Log("Fetching map.. " + url);
		fetchOrigo = origo;
	}
	
	/// <summary>
	/// Draws the texture obtained from the Google Static Maps onto a circle with the player
	/// and target positions.
	/// </summary>
	/// <param name='selfCoords'>
	/// The player's coordinates.
	/// </param>
	/// <param name='bearing'>
	/// The current bearing of the player.
	/// </param>
	/// <param name='targetCoords'>
	/// The target's coordinates.
	/// </param>
	private void GUIMap(Position selfCoords, double bearing, Position targetCoords) {
		// Get the direction the player is running in.
		Position direction = new Position(selfCoords.latitude + (float)(Math.Cos(bearing)*1000/111229d), 
		                                  selfCoords.longitude + (float)(Math.Sin(bearing)*1000/111229d));	
		
		// Get the bearing in pixel format.
		double pixelBearing = Angle(MercatorToPixel(selfCoords), MercatorToPixel(direction));	
		bearing = pixelBearing;
		
		// Get a static map with a radius of mapAtlasRadius, cache and re-get if viewport within margin of the border
		const int margin = 15;	
		int maxdrift = (mapAtlasRadius-MAP_RADIUS-margin);
		Vector2 drift = MercatorToPixel(mapOrigo) - MercatorToPixel(selfCoords);
		if (mapWWW == null && (mapTexture == null || drift.magnitude >= maxdrift)) {
			FetchMapTexture(selfCoords);
			UnityEngine.Debug.Log("Map: fetched");
		}
		if (mapWWW != null && mapWWW.isDone) {
			if (mapWWW.error != null) {
				debugText = mapWWW.error;
				UnityEngine.Debug.Log(mapWWW.error);
				UnityEngine.Debug.Log("Map: error with map");
			} else {
				debugText = "";
				mapTexture = mapWWW.texture;
				mapOrigo = fetchOrigo;
				UnityEngine.Debug.Log("Map: origo error");
			}
			mapWWW = null;
		}
		if (mapTexture == null) {
			GUI.Label(map, "Fetching map..");
			return;
		}
		
		// TODO: Stencil out circle
		Color original = GUI.color;
		GUI.color = new Color(1f, 1f, 1f, OPACITY);
		
		// Map self coordinates into map atlas, normalize to atlas size and shift to center
		Vector2 mapNormalSelf = (MercatorToPixel(mapOrigo) - MercatorToPixel(selfCoords)) / (mapAtlasRadius*2);
		mapNormalSelf.x += 0.5f;
		mapNormalSelf.y += 0.5f;
		float normalizedRadius = (float)MAP_RADIUS/(mapAtlasRadius*2);
		// Draw a MAP_RADIUS-sized circle around self
		Rect mapCoords = new Rect(1 - mapNormalSelf.x - normalizedRadius, mapNormalSelf.y - normalizedRadius,
		                          normalizedRadius*2, normalizedRadius*2);
		Vector2 mapCenter = new Vector2(map.x + map.width/2, map.y + map.height/2);
		Matrix4x4 matrixBackup = GUI.matrix;
		if (Event.current.type == EventType.Repaint) {
			// Rotation and indexing into atlas handled by shader
			mapStencil.SetFloat("_Rotation", (float)-bearing);			
			mapStencil.SetVector("_Rectangle", new Vector4(mapCoords.x, mapCoords.y, mapCoords.width, mapCoords.height));
//			Graphics.DrawTexture(map, mapTexture, mapCoords, 0, 0, 0, 0, mapStencil);
			Graphics.DrawTexture(map, mapTexture, mapStencil);
		}
		
		// Self is always at center
		mapSelf.x = mapCenter.x - mapSelf.width/2;
		mapSelf.y = mapCenter.y - mapSelf.height/2;
		GUI.DrawTexture(mapSelf, selfIcon);
		// Target is relative to self and limited to map radius
		Vector2 localTarget = MercatorToPixel(selfCoords) - MercatorToPixel(targetCoords);
		if (localTarget.magnitude > MAP_RADIUS) {
			localTarget.Normalize();
			localTarget *= MAP_RADIUS;
			// TODO: Change icon to indicate outside of minimap?
		}		
		
		// Rotated so bearing is up
		double c = Math.Cos(-bearing);
		double s = Math.Sin(-bearing);
		mapTarget.x = mapCenter.x - (float)(localTarget.x * c - localTarget.y * s)  - mapTarget.width/2;
		mapTarget.y = mapCenter.y - (float)(localTarget.x * s + localTarget.y * c) - mapTarget.height/2;
		GUI.DrawTexture(mapTarget, targetIcon);
				
		GUI.matrix = matrixBackup;
		GUI.color = original;
	}
	
	/// <summary>
	/// Converts a Mercator coordinate to a Vector2 for the pixel position.
	/// </summary>
	/// <returns>
	/// The converted pixel..
	/// </returns>
	/// <param name='mercator'>
	/// The initial Mercator transformed position.
	/// </param>
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
		return world * scale;
	}
	
	/// <summary>
	/// Changes a pixel coordinate to a Mercator projected coordinate.
	/// </summary>
	/// <returns>
	/// The Mercator coordinate.
	/// </returns>
	/// <param name='pixel'>
	/// The initial pixel coordinate. 
	/// </param>
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
	
	/// <summary>
	/// Obtains the angle between two Vectors.
	/// </summary>
	/// <param name='pos1'>
	/// The first vector.
	/// </param>
	/// <param name='pos2'>
	/// The second vector.
	/// </param>
	private double Angle(Vector2 pos1, Vector2 pos2) {
	    Vector2 from = pos2 - pos1;
	    Vector2 to = new Vector2(0, 1);
	 
	    float result = Vector2.Angle( from, to );
		
	    Vector3 cross = Vector3.Cross( from, to );
	 
	    if (cross.z > 0)
	       result = 360f - result;
		
		result += 180.0f;
		
	    return result*Math.PI/180;
	}
	
	/// <summary>
	/// Sets the offset of the target.
	/// </summary>
	/// <param name='off'>
	/// The offset value.
	/// </param>
	public void SetOffset(float off) {
		offset = off;
	}
	
	/// <summary>
	/// Converts the distance from meters to miles.
	/// </summary>
	/// <returns>
	/// The distance in miles.
	/// </returns>
	/// <param name='meters'>
	/// The distance in meters.
	/// </param>
	string SiDistance(double meters) {
		string postfix = " Miles";
		string final;
		float value = (float)meters/1609.344f;
		final = value.ToString("f2");
		return final+postfix;
	}
}
