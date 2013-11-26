using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class TrackSelect : MonoBehaviour {
	
	//private Platform inputData;
	private float originalWidth = 800;
	private float originalHeight = 500;
	public static Texture2D lineTex;
	private Vector3 scale;
	private List<Track> trackList;
	private int currentTrack = 0;
	Texture2D normal;
	private bool started = false;
	private bool active = true;
	
	private Rect map;
	private const int MAP_RADIUS = 100;
	private const int MARGIN = 15;
	private const float OPACITY = 0.5f;
	Texture2D mapTexture = null;
	Material mapStencil;
	const int mapAtlasRadius = 315; // API max width/height is 640
	int mapZoom = 18;
	WWW mapWWW = null;
	private bool mapChanged = true;
	
	private float latHigh;
	private float latLow;
	private float longHigh;
	private float longLow;
	private float centerLat;
	private float centerLong;
	
	private bool changed;
	
	// Use this for initialization
	void Start () {
		trackList = Platform.Instance.getTracks();
	}
	
	public bool isChanged() {
		return changed;
	}
	
	public void setChanged(bool c) {
		changed = c;
	}
	
	void OnGUI() {
		
//		Matrix4x4 defaultMatrix = GUI.matrix;
//		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
//		//GUI.depth = 10;
//		GUI.skin.box.wordWrap = true;
//		GUI.skin.box.fontSize = 30;
//		GUI.depth = 3;
//		GUI.skin.box.fontStyle = FontStyle.Bold;
//		GUI.skin.box.alignment = TextAnchor.MiddleCenter;				
//		GUI.skin.box.normal.background = normal;
//		GUI.skin.box.normal.textColor = Color.black;
			
//		if(GUI.Button(new Rect(0, originalHeight/2, 100, 100), "Previous Track")) {
//			Platform.Instance.getPreviousTrack();
//			curTrackPositions = Platform.Instance.getTrackPositions();
//			GetLimits();
//			mapChanged = true;
//			mapTexture = null;
//		}
//		
//		if(GUI.Button(new Rect(originalWidth-100, originalHeight/2, 100, 100), "Next Track")) {
//			Platform.Instance.getNextTrack();
//			curTrackPositions = Platform.Instance.getTrackPositions();
//			GetLimits();
//			mapChanged = true;
//			mapTexture = null;
//		}
//		
//		if(GUI.Button(new Rect(originalWidth-100, originalHeight-100, 100, 100), "Set Track")) {
//			Platform.Instance.setTrack();
//			changed = true;
//		}
//		
//		if(GUI.Button(new Rect(originalWidth-100, 0, 100, 100), "Get Tracks")) {
//			GetTracks();
//		}
			
//		if(started) {
//		
//			double sin = Math.Sin(latHigh * Math.PI / 180);
//			double radX2 = Math.Log((1 + sin) / (1-sin)) / 2;
//			double neLat = Math.Max(Math.Min(radX2, Math.PI), -Math.PI) /2;
//		
//			sin = Math.Sin(latLow * Math.PI / 180);
//			radX2 = Math.Log((1 + sin) / (1-sin)) / 2;
//			double swLat = Math.Max(Math.Min(radX2, Math.PI), -Math.PI) / 2;
//		
//			double latFraction = (neLat - swLat) / Math.PI;
//		
//			double longDiff = longHigh - longLow;
//			double longFraction = ((longDiff < 0) ? (longDiff + 360) : longDiff) / 360;
//		
//			double latZoom = Math.Floor (Math.Log(600 / 256 / latFraction) / 0.6931471805599453 );
//			double longZoom = Math.Floor(Math.Log(400  / 256 / longFraction) / 0.6931471805599453);
//			
//			UnityEngine.Debug.Log("Zoom: Lat Zoom = " + latZoom.ToString());
//			UnityEngine.Debug.Log("Zoom: long zoom = " + longZoom.ToString());
//		
//			mapZoom = (int)Math.Min(latZoom, longZoom);
//			if(mapZoom > 21) {
//				mapZoom = 21;
//			}
//		
//			if ((mapWWW == null && mapTexture == null) && mapChanged){
//				const string API_KEY = "AIzaSyBj_iHOwteDxJ8Rj_bPsoslxIquy--y9nI";
//				const string endpoint = "http://maps.googleapis.com/maps/api/staticmap";
//				string url = endpoint + "?center="
//			                      	  + centerLat.ToString() + "," + centerLong.ToString()
//			               	      	  + "&zoom=" + mapZoom.ToString()
//			  	                  	  + "&size=" + "600" + "x" + "400"
//			       	              	  + "&maptype=roadmap" 
//									  + "&sensor=true&key=" + API_KEY;
//				mapWWW = new WWW(url);
//				UnityEngine.Debug.Log("Map: URL is: " + url);
//				}
//			if (mapWWW != null && mapWWW.isDone) {
//				if (mapWWW.error != null) {
//				//debugText = mapWWW.error;
//					UnityEngine.Debug.LogWarning(mapWWW.error);
//					UnityEngine.Debug.LogWarning("Map: error with map");
//				} else {
//				//debugText = "";
//					mapTexture = mapWWW.texture;
//					mapChanged = false;
//				//mapOrigo = fetchOrigo;
//					UnityEngine.Debug.Log("Map: origo error");
//				}
//				mapWWW = null;
//			}
//						
//			if (mapTexture == null) {
//				GUI.Label(map, "Fetching map...    Number of Positions = " + curTrackPositions.Count.ToString());
//			}
//				GUI.DrawTexture(map, mapTexture);
//				
//			GUI.matrix = Matrix4x4.identity;
//			
//			Position center = new Position(centerLat, centerLong);
//			
//			for(int i=0; i<curTrackPositions.Count - 1; i++) {
//				//UnityEngine.Debug.Log("Track: current track position = " + curTrackPositions[i].latitude + ", " + curTrackPositions[i].longitude);
//				Vector2 currentPos = mercatorToPixel(curTrackPositions[i]) - mercatorToPixel(center); 
//				currentPos += new Vector2(400.0f * scale.x, 250.0f * scale.y);
//				
//				Vector2 nextPos = mercatorToPixel(curTrackPositions[i+1]) - mercatorToPixel(center);
//				nextPos += new Vector2(400.0f * scale.x, 250.0f * scale.y);
//				
//				if(currentPos != nextPos) {
//					//UnityEngine.Debug.Log("Track: Current position = " + currentPos.x.ToString() + ", " + currentPos.y.ToString());
//					//UnityEngine.Debug.Log("Track: Next position = " + nextPos.x.ToString() + ", " + nextPos.y.ToString());
//					
//					DrawLine(currentPos, nextPos, Color.black, 10.0f);	
//				}
//			}
		
		
		
	}
	
	void Update() {
		if(currentTrack != Platform.Instance.currentTrack) {
				UnityEngine.Debug.Log("TrackPanel: track change: " + Platform.Instance.currentTrack);
			mapChanged = true;
			if(Platform.Instance.currentTrack > trackList.Count - 1 ) {
				Platform.Instance.currentTrack = 0;
			}
			
			if(Platform.Instance.currentTrack < 0) {
				Platform.Instance.currentTrack = trackList.Count - 1;
			}
			
			if (trackList.Count == 0) Platform.Instance.currentTrack = 0;
			
			currentTrack = Platform.Instance.currentTrack;
				UnityEngine.Debug.Log("TrackPanel: current track: " + currentTrack);
		}
		
		if(mapChanged) {
			GetLimits();
			mapTexture = null;
			
			double sin = Math.Sin(latHigh * Math.PI / 180);
			double radX2 = Math.Log((1 + sin) / (1-sin)) / 2;
			double neLat = Math.Max(Math.Min(radX2, Math.PI), -Math.PI) /2;
		
			sin = Math.Sin(latLow * Math.PI / 180);
			radX2 = Math.Log((1 + sin) / (1-sin)) / 2;
			double swLat = Math.Max(Math.Min(radX2, Math.PI), -Math.PI) / 2;
		
			double latFraction = (neLat - swLat) / Math.PI;
		
			double longDiff = longHigh - longLow;
			double longFraction = ((longDiff < 0) ? (longDiff + 360) : longDiff) / 360;
		
			double latZoom = Math.Floor (Math.Log(600 / 256 / latFraction) / 0.6931471805599453 );
			double longZoom = Math.Floor(Math.Log(400  / 256 / longFraction) / 0.6931471805599453);
			
			UnityEngine.Debug.Log("Zoom: Lat Zoom = " + latZoom.ToString());
			UnityEngine.Debug.Log("Zoom: long zoom = " + longZoom.ToString());
		
			mapZoom = (int)Math.Min(latZoom, longZoom);
			if(mapZoom > 18) {
				mapZoom = 18;
			}
		
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
			
			if (mapWWW != null) {
				while(!mapWWW.isDone) {
					if (mapWWW.error != null) {
						UnityEngine.Debug.LogWarning(mapWWW.error);
						UnityEngine.Debug.LogWarning("Map: error with map");
						break;
					} 
				}
				
				mapTexture = mapWWW.texture;
				
				mapWWW = null;
			}
						
			UITexture tex = GetComponent<UITexture>();
			tex.mainTexture = mapTexture;
			mapChanged = false;
		}
	}
	
	public void GetTracks() {
		Platform.Instance.getTracks();
		//curTrackPositions = Platform.Instance.getTrackPositions();
		started = true;
		mapChanged = true;
		mapTexture = null;
		GetLimits();
		
	}
	
	public Track CurrentTrack() {
		return trackList[currentTrack];
	}
	
	void GetLimits() {
		float newlatLow = 360;
		float newlongLow = 360;
		float newlatHigh = -360;
		float newlongHigh = -360;
		float totalLat = 0;
		float totalLong = 0;
		
		List<Position> curTrackPositions = trackList[currentTrack].trackPositions;
		
		for(int i=0; i<curTrackPositions.Count; i++)
		{
			if(curTrackPositions[i].latitude < newlatLow) {
				newlatLow = curTrackPositions[i].latitude;
			}
			
			if(curTrackPositions[i].latitude > newlatHigh) {
				newlatHigh = curTrackPositions[i].latitude;
			}
			
			if(curTrackPositions[i].longitude < newlongLow) {
				newlongLow = curTrackPositions[i].longitude;
			}
			
			if(curTrackPositions[i].longitude > newlongHigh) {
				newlongHigh = curTrackPositions[i].longitude;
			}
			totalLat += curTrackPositions[i].latitude;
			totalLong += curTrackPositions[i].longitude;
		}
		float newcenterLat = totalLat / curTrackPositions.Count;
		float newcenterLong = totalLong / curTrackPositions.Count;
		
		if(newcenterLat == centerLat && newcenterLong == centerLong && newlatHigh == latHigh && newlatLow == latLow && newlongLow == longLow && newlongHigh == longHigh)
		{
			mapChanged = true;
			centerLat = newcenterLat;
			centerLong = newcenterLong;
			latHigh = newlatHigh;
			latLow = newlatLow;
			longHigh = newlongHigh;
			longLow = newlongLow;
		}
		
		Position cent = new Position(centerLat, centerLong);
		Position ne = new Position(latHigh, longHigh);
		Position  sw = new Position(latLow, longLow);
		UnityEngine.Debug.Log("Track: Center lat + long are: " + cent.latitude + ", " + cent.longitude);
		UnityEngine.Debug.Log("Track: NE lat + long are: " + ne.latitude + ", " + ne.longitude);
		UnityEngine.Debug.Log("Track: SW lat + long are: " + sw.latitude + ", " + sw.longitude);
		
	}
	
	Vector2 mercatorToPixel(Position mercator) {
		// Per google maps spec: pixelCoordinate = worldCoordinate * 2^zoomLevel
		Vector2 mapScale = scale * (int)Math.Pow(2, mapZoom);
		
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
		return new Vector2(world.x * mapScale.x, world.y * mapScale.y);
	}
	
	public void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width) {
		Matrix4x4 matrix = GUI.matrix;
 
        // Determine the angle of the line.
        float angle = Vector3.Angle(pointB - pointA, Vector2.right);
 
        // Vector3.Angle always returns a positive number.
        // If pointB is above pointA, then angle needs to be negative.
        if (pointA.y > pointB.y) { angle = -angle; }
 		//UnityEngine.Debug.Log("Draw Line: Angle = " + angle.ToString());
        // Use ScaleAroundPivot to adjust the size of the line.
        // We could do this when we draw the texture, but by scaling it here we can use
        //  non-integer values for the width and length (such as sub 1 pixel widths).
        // Note that the pivot point is at +.5 from pointA.y, this is so that the width of the line
        //  is centered on the origin at pointA.
		Vector2 scalePivot = new Vector2((pointB - pointA).magnitude, width);
		//UnityEngine.Debug.Log("Draw Line: scale pivot is: " + scalePivot.x.ToString() + ", " + scalePivot.y.ToString());
        
		GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));
 
        // Set the rotation for the line.
        //  The angle was calculated with pointA as the origin.
        GUIUtility.RotateAroundPivot(angle, pointA);
 
        // Finally, draw the actual line.
        // We're really only drawing a 1x1 texture from pointA.
        // The matrix operations done with ScaleAroundPivot and RotateAroundPivot will make this
        //  render with the proper width, length, and angle.
		//UnityEngine.Debug.Log("Draw Line: Point A is " + pointA.x.ToString() + ", " + pointA.y.ToString());
       // GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), blackTex);
 		
		
        // We're done.  Restore the GUI matrix and GUI color to whatever they were before.
        GUI.matrix = matrix;
	}
}
