using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using System.Collections;

using RaceYourself.Models;

/// <summary>
/// This class is used to display the map and control the Track Select screen.
/// </summary>
public class TrackSelect : MonoBehaviour {
	
	// Texture for the line drawing
	public static Texture2D lineTex;
	
	// Tracklist
	private List<Track> trackList;
	
	// Current track to display
	private int currentTrack = 0;
	Texture2D normal;
	
	// Map attributes
	private Rect map;
	private const int MAP_RADIUS = 100;
	Texture2D mapTexture = null;
	Material mapStencil;
	const int mapAtlasRadius = 315; // API max width/height is 640
	int mapZoom = 18;
	WWW mapWWW = null;
	private bool mapChanged = true;	
	
	// Constants for the margin and opacity
	private const int MARGIN = 15;
	private const float OPACITY = 0.5f;
	
	// Limits and center for the latitude, longitude.
	private float latHigh;
	private float latLow;
	private float longHigh;
	private float longLow;
	private float centerLat;
	private float centerLong;
	
	// Texture for the map
	private UITexture tex;
	
	private GestureHelper.OnTap tapHandler = null;
	
	private GestureHelper.OnSwipeLeft leftHandler = null;
	
	private GestureHelper.OnSwipeRight rightHandler = null;
	
	private GestureHelper.OnBack backHandler = null;
	
	/// <summary>
	/// Start this instance. Gets the tracklist
	/// </summary>
	void Start () {
		trackList = (List<Track>)DataVault.Get("track_list");
		UnityEngine.Debug.Log("TrackSelect: There are " + trackList.Count + " tracks");
		
		tex = GetComponent<UITexture>();
		
		LoadTrack();
		
		tapHandler = new GestureHelper.OnTap(() => {
			UnityEngine.Debug.Log("TrackSelect: setting track");
			SetTrack();
		});
		
		GestureHelper.onTap += tapHandler;
		
		leftHandler = new GestureHelper.OnSwipeLeft(() => {
			UnityEngine.Debug.Log("TrackSelect: getting previous track");
			PreviousTrack();
		});
		
		GestureHelper.onSwipeLeft += leftHandler;
		
		rightHandler = new GestureHelper.OnSwipeRight(() => {
			UnityEngine.Debug.Log("TrackSelect: getting next track");
			NextTrack();
		});
		
		GestureHelper.onSwipeRight += rightHandler;
		
		backHandler = new GestureHelper.OnBack(() => {
			GoBack();
		});
		
		GestureHelper.onBack += backHandler;
	}
	
	public void GoBack() 
	{
		FlowStateBase.FollowBackLink();
		GestureHelper.onTap -= tapHandler;
		GestureHelper.onSwipeLeft -= leftHandler;
		GestureHelper.onSwipeRight -= rightHandler;
		GestureHelper.onBack -= backHandler;
	}
	
	public void NextTrack() 
	{
		currentTrack++;
		if(currentTrack >= trackList.Count)
		{
			currentTrack = 0;
		}
		
		LoadTrack();
	}
	
	public void PreviousTrack()
	{
		currentTrack--;
		if(currentTrack < 0) 
		{
			currentTrack = trackList.Count - 1;
		}
		
		LoadTrack();
	}
	
	public void LoadTrack()
	{
		GetLimits();
			
		DataVault.Set("track_distance", SiDistanceUnitless(trackList[currentTrack].distance));
		DataVault.Set("track_time", TimestampMMSS(trackList[currentTrack].time));
		DataVault.Set("track_date", trackList[currentTrack].date);
		
		// Remove the old texture
		mapTexture = null;
			
		// The next section calculates the zoom for the static maps. 
		CalculateMapZoom();
		
		StartCoroutine(GetMap());
		
	}
	
	IEnumerator GetMap() {
		if (mapWWW == null && mapTexture == null){
			const string API_KEY = "AIzaSyBj_iHOwteDxJ8Rj_bPsoslxIquy--y9nI";
			const string endpoint = "http://maps.googleapis.com/maps/api/staticmap";
			string url = endpoint + "?center="
		                      	  + centerLat.ToString() + "," + centerLong.ToString()
		               	      	  + "&zoom=" + mapZoom.ToString()
		  	                  	  + "&size=" + "600" + "x" + "400"
		       	              	  + "&maptype=roadmap" 
								  + "&sensor=true&key=" + API_KEY;
			mapWWW = new WWW(url);
			UnityEngine.Debug.Log("TrackSelect: URL is: " + url);
		}
			
			// If the website worked.
		if (mapWWW != null) {
			// We wait for the map to download.			
			yield return mapWWW;
				
			// The texture is then set.
			mapTexture = mapWWW.texture;
				
			// The link is then nulled.
			mapWWW = null;
		}
		
		tex.mainTexture = mapTexture;
	}
	
	protected string TimestampMMSS(long milli) {
		TimeSpan span = TimeSpan.FromMilliseconds(milli);

		return string.Format("{0:00}:{1:00}",span.Minutes,span.Seconds);	
	}
	
	public void SetTrack() {
		DataVault.Set("current_track", trackList[currentTrack]);
		FlowStateBase fs = FlowStateMachine.GetCurrentFlowState();
		GConnectorBase gConnect = fs.Outputs.Find(r => r.Name == "GameExit");
		if(gConnect != null)
		{
			GestureHelper.onTap -= tapHandler;
			GestureHelper.onSwipeLeft -= leftHandler;
			GestureHelper.onSwipeRight -= rightHandler;
			GestureHelper.onBack -= backHandler;
			(gConnect.Parent as Panel).CallStaticFunction(gConnect.EventFunction, null);
			fs.parentMachine.FollowConnection(gConnect);
		} else 
		{
			UnityEngine.Debug.Log("TrackSelect: Connection not found");
		}
	}
	
	public void CalculateMapZoom() 
	{
		// First, the north-east latitude is calculated by finding the 
		// sine of the latitude. Next the log of the value is found and
		// then the value is set based on which value is higher.
		double sin = Math.Sin(latHigh * Math.PI / 180);
		double radX2 = Math.Log((1 + sin) / (1-sin)) / 2;
		double neLat = Math.Max(Math.Min(radX2, Math.PI), -Math.PI) /2;
		
		// The same is then performed to the south west latitude
		sin = Math.Sin(latLow * Math.PI / 180);
		radX2 = Math.Log((1 + sin) / (1-sin)) / 2;
		double swLat = Math.Max(Math.Min(radX2, Math.PI), -Math.PI) / 2;
		
		// A fraction is obtained from dividing the distance by Pi
		double latFraction = (neLat - swLat) / Math.PI;
		
		// Next we calculate the longitude by finding the difference between
		// the two and then obtaining a fraction.
		double longDiff = longHigh - longLow;
		double longFraction = ((longDiff < 0) ? (longDiff + 360) : longDiff) / 360;
		
		// The latitude zoom is then calculated based on the pixel width / map tile width / latitude fraction / ln2
		double latZoom = Math.Floor (Math.Log(600 / 256 / latFraction) / 0.6931471805599453 );
		
		// The latitude zoom is then calculated based on the pixel height / map tile height / longitude fraction / ln2
		double longZoom = Math.Floor(Math.Log(400  / 256 / longFraction) / 0.6931471805599453);
			
		UnityEngine.Debug.Log("Zoom: Lat Zoom = " + latZoom.ToString());
		UnityEngine.Debug.Log("Zoom: long zoom = " + longZoom.ToString());
		
		// Finally, the zoom is set based on the smaller value.
		mapZoom = (int)Math.Min(latZoom, longZoom);
		if(mapZoom > 18) {
			mapZoom = 18;
		}
	}
	
	/// <summary>
	/// Gets the limits of latitude and logitude as well as the center.
	/// </summary>
	void GetLimits() {
		// Set to impossible values so will straight away be overtaken
		float newlatLow = 360;
		float newlongLow = 360;
		float newlatHigh = -360;
		float newlongHigh = -360;
		float totalLat = 0;
		float totalLong = 0;
		
		// Get the list of positions for the current track
		List<Position> curTrackPositions = trackList[currentTrack].positions;
		
		// Loop through all positions and test for limits
		for(int i=0; i<curTrackPositions.Count; i++)
		{
			// Check for the lowest latitude
			if(curTrackPositions[i].latitude < newlatLow) {
				newlatLow = curTrackPositions[i].latitude;
			}
			
			// Check for the highest latitude
			if(curTrackPositions[i].latitude > newlatHigh) {
				newlatHigh = curTrackPositions[i].latitude;
			}
			
			// Check for the lowest longitude
			if(curTrackPositions[i].longitude < newlongLow) {
				newlongLow = curTrackPositions[i].longitude;
			}
			
			// Check for the highest longitude
			if(curTrackPositions[i].longitude > newlongHigh) {
				newlongHigh = curTrackPositions[i].longitude;
			}
			
			// Add to the total lat and long
			totalLat += curTrackPositions[i].latitude;
			totalLong += curTrackPositions[i].longitude;
		}
		
		// Calculate the average lat and long for the center
		float newcenterLat = totalLat / curTrackPositions.Count;
		float newcenterLong = totalLong / curTrackPositions.Count;
		
		// Set the map changed to true
		mapChanged = true;
		
		// Set the center positions
		centerLat = newcenterLat;
		centerLong = newcenterLong;
		
		// Set the limits of the latitude and longitude
		latHigh = newlatHigh;
		latLow = newlatLow;
		longHigh = newlongHigh;
		longLow = newlongLow;
		 
		// Print out the position
		Position cent = new Position(centerLat, centerLong);
		Position ne = new Position(latHigh, longHigh);
		Position  sw = new Position(latLow, longLow);
		UnityEngine.Debug.Log("Track: Center lat + long are: " + cent.latitude + ", " + cent.longitude);
		UnityEngine.Debug.Log("Track: NE lat + long are: " + ne.latitude + ", " + ne.longitude);
		UnityEngine.Debug.Log("Track: SW lat + long are: " + sw.latitude + ", " + sw.longitude);
		
	}
	
	/// <summary>
	/// Converts a Mercator Projection coordinate to pixel.
	/// </summary>
	/// <returns>
	/// The coordinate in pixels.
	/// </returns>
	/// <param name='mercator'>
	/// The coordinate in lat and long.
	/// </param>
	Vector2 MercatorToPixel(Position mercator) {
		// Per google maps spec: pixelCoordinate = worldCoordinate * 2^zoomLevel
		int mapScale = (int)Math.Pow(2, mapZoom);
		
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
		return world * mapScale;
	}
	
	/// <summary>
	/// Draws a 2D line.
	/// </summary>
	/// <param name='pointA'>
	/// Start point.
	/// </param>
	/// <param name='pointB'>
	/// End Point.
	/// </param>
	/// <param name='color'>
	/// Color of the line.
	/// </param>
	/// <param name='width'>
	/// Width of the line.
	/// </param>
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
	
	protected string SiDistanceUnitless(double meters) {
		string postfix = "m";
		string final;
		float value = (float)meters;
		if (value > 1000) {
			value = value/1000;
			postfix = "km";
			if(value >= 10) {
				final = value.ToString("f1");
			} else {
				final = value.ToString("f2");
			}
		}
		else
		{
			final = value.ToString("f0");
		}
		//set the units string for the HUD
		
		return final + postfix;
	}
}
