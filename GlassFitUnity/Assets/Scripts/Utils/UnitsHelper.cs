using UnityEngine;
using System.Collections;
using System;

using RaceYourself.Models;

/// <summary>
/// Units helper.
/// A collection of utility functions for formatting and operating on times and distances
/// Originally in GameBase.cs
/// </summary>

public class UnitsHelper {

	/// <summary>
	/// converts a time duration to a string of the form hh:mm:ss or mm:ss if the duration is less than 1 hour
	/// </summary>
	/// <returns>
	/// The MMSS from M.
	/// </returns>
	public static string TimestampMMSSFromMS(long milliseconds) {
		//UnityEngine.Debug.Log("Converting Timestamp in milliseconds" + milliseconds);
		TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);
		//if we're into hours, show them
		if(span.Hours > 0)
		{
			return string.Format("{0:0}:{1:00}:{2:00}", span.Hours, span.Minutes, span.Seconds);
		}
		else
		{
			return string.Format("{0:0}:{1:00}",span.Hours*60 + span.Minutes, span.Seconds);
		}
	}
	
		
	/// <summary>
	/// Get a suitably formatted string to represent a raw distance in km or m as appropriate.
	/// </summary>
	/// <returns>
	/// The distance.
	/// </returns>
	/// <param name='meters'>
	/// Meters.
	/// </param>
	public static string SiDistance(double meters) {
		return SiDistanceUnitless(meters, "distanceunits") + DataVault.Get("distance_units");
	}

	/// <summary>
	/// As above but doesn't use DataVault
	/// </summary>
	/// <returns>The distance string in full.</returns>
	/// <param name="meters">Meters.</param>
	public static string SiDistanceFull(double meters) {
		string postfix = "m";
		string final;
		float value = (float)meters;
		if(value > 1000) {
			value = value / 1000;
			postfix = "km";
			if(value >= 10) {
				final = value.ToString("f1");
			} else {
				final = value.ToString("f2");
			}
		} else {
			final = value.ToString("f0");
		}

		return final + postfix;
	}

	public static Vector2 MercatorToPixel(Position mercator, int mapZoom) {
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
	/// As above, but leaves the units off the string, storing them in the dataVault with the specified key
	/// </summary>
	/// <returns>
	/// The distance without units
	/// </returns>
	/// <param name='meters'>
	/// Meters.
	/// </param>
	/// <param name='units'>
	/// Units.
	/// </param>
	public static string SiDistanceUnitless(double meters, string unitsKey) {
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
		DataVault.Set(unitsKey, postfix);
		return final;
	}
	
	/// <summary>
	/// Converts raw speed to a pace per km.
	/// </summary>
	/// <returns>
	/// The pace in minutes per km
	/// </returns>
	/// <param name='speed'>
	/// the raw speed in metres per second
	/// </param>
	public static float SpeedToKmPace(float speed) {
		if (speed <= 0) {
			return 0;
		}
		// m/s -> mins/Km
		return ((1/speed)/60) * 1000;
	}
	
	/// <summary>
	/// converts a time duration in milliseconds to a string in either hh:mm:ss:ds format or mm:ss:dd format. 'ds' representing 1/100ths of a second.
	/// Also stores the units chosen in the dataVault with key "time_units"
	/// </summary>
	/// <returns>
	/// The formatted string representing the time
	/// </returns>
	/// <param name='milliseconds'>
	/// The time duration in milliseconds
	/// </param>
	public static string TimestampMMSSdd(long milliseconds) {
		TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);
		//if we're into hours, show them
		if(span.Hours > 0)
		{
			return string.Format("{0:0}:{1:00}:{2:00}:{3:00}", span.Hours, span.Minutes, span.Seconds, span.Milliseconds/10);
			//set units string for HUD
			DataVault.Set("time_units", "h:m:s");
		}
		else
		{				
			return string.Format("{0:0}:{1:00}:{2:00}",span.Hours*60 + span.Minutes, span.Seconds, span.Milliseconds/10);
			//set units string for HUD
			DataVault.Set("time_units", "m:s:ds");
		}
			
	}
	
	/// <summary>
	/// convert a duration in minutes to a string representation in hh:mmm format
	/// </summary>
	/// <returns>
	/// The formatted string
	/// </returns>
	/// <param name='minutes'>
	/// the duration in minutes
	/// </param>
	public static string TimestampMMSS(long minutes) {
		TimeSpan span = TimeSpan.FromMinutes(minutes);

		return string.Format("{0:00}:{1:00}",span.Minutes,span.Seconds);	
	}
	
	/// <summary>
	/// Convert a duration in minutes to a string, rounded to the nearest 10 seconds, and formatted appropriately
	/// </summary>
	/// <returns>
	/// The MMS snearest ten secs.
	/// </returns>
	/// <param name='mins'>
	/// Mins.
	/// </param>
	public static string TimestampMMSSnearestTenSecs(float mins) {
		TimeSpan span = TimeSpan.FromMinutes(mins);
		int minutes = span.Minutes;
		int seconds = (int)Math.Ceiling(span.Seconds / 10.0f) * 10; // ceil to nearest 10
		return string.Format("{0:00}:{1:00}", minutes, seconds);
	}
	
	/// <summary>
	/// Convert a duration in milliseconds to a string in m:s or h:m:s
	/// </summary>
	/// <returns>
	/// The MMS sfrom millis.
	/// </returns>
	/// <param name='milliseconds'>
	/// Milliseconds.
	/// </param>
	public static string TimestampMMSSfromMillis(long milliseconds) {
		TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);
		
		if(span.Hours > 0)
		{
			DataVault.Set("time_units", "m:s");
			return string.Format("{0:0}:{1:00}", span.Minutes, span.Seconds);
		} else {
			DataVault.Set("time_units", "h:m:s");
			return string.Format("{0:0}:{1:00}:{2:00}", span.Hours, span.Minutes, span.Seconds);
		}
	}
	
	public static long MillisSince1970(DateTime datetime) {
		return (long)((datetime.ToUniversalTime() - new DateTime (1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);
	}
	
	
}
