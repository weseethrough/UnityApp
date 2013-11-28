using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.IO;

public class PlatformDummy {
	
	private Stopwatch timer = new Stopwatch();
	private System.Random random = new System.Random();
	private long update = 0;
	private int distance = 0;
	private int target = 1;
	private Position position = null;
	private float bearing = 45; // degrees	
	
	private const float weight = 75.0f;
	private const float factor = 1.2f;
	
	private string blobstore = "editor-blobstore";
	private string blobassets = "blob";
	
	public PlatformDummy() {
		timer.Start();
		blobstore = Path.Combine(Application.persistentDataPath, blobstore);
		Directory.CreateDirectory(blobstore);
		blobassets = Path.Combine(Application.streamingAssetsPath, blobassets);
		Directory.CreateDirectory(blobassets);
		UnityEngine.Debug.Log("Editor blobstore: " + blobstore + ", blobassets: " + blobassets);
		SimpleJSON.JSONArray n = SimpleJSON.JSON.Parse("[ {'device_id':'1878722582', 'track_id':'11', 'user_id':'10'} ]".Replace("'", "\"")).AsArray;
		UnityEngine.Debug.Log("Moo: " + n[0]["device_id"].AsInt);
	}
	
	public void StartTrack(bool indoor) {
		throw new NotImplementedException();
	}
	
	public Boolean HasLock() {
		return true;
	}
	
	public void StopTrack() {
		throw new NotImplementedException();
	}
	
	public void Reset() {
		throw new NotImplementedException();
	}
	
	public void SetTargetSpeed(float speed)
	{
		throw new NotImplementedException();
	}
	
	public void SetTargetTrack(int trackID)
	{
		throw new NotImplementedException();
	}
	
	public Friend[] Friends() {
		var friend = @"{
	        ""_id"": ""gplus107650962788507404146"",
	        ""has_glass"": false,
	        ""image"": ""https://lh6.googleusercontent.com/-c89V0_0E6tM/AAAAAAAAAAI/AAAAAAAAAFE/9oLaR0rjbog/photo.jpg?sz=50"",
	        ""name"": ""Aaron Aycock"",
	        ""photo"": null,
	        ""uid"": ""107650962788507404146"",
	        ""user_id"": null
	      }";
		Friend[] friends = new Friend[1];
		friends[0] = new Friend(friend);
		return friends;
	}
	
	public byte[] LoadBlob(string id) {
		try {
			if (!File.Exists(Path.Combine(blobstore, id))) {
				return File.ReadAllBytes(Path.Combine(blobassets, id));
			}
			return File.ReadAllBytes(Path.Combine(blobstore, id));
		} catch (FileNotFoundException e) {
			return new byte[0];
		}
	}
	
	public void StoreBlob(string id, byte[] blob) {
		File.WriteAllBytes(Path.Combine(blobstore, id), blob);
	}
	
	public void EraseBlob(string id) {
		File.Delete(Path.Combine(blobstore, id));
	}
		
	/**
	 * Editor-specific function. 
	 */
	public void StoreBlobAsAsset(string id, byte[] blob) {
		File.WriteAllBytes(Path.Combine(blobassets, id), blob);
	}
	
	public void Poll() {
		if (!timer.IsRunning) return;
		if (Time() - update > 1000) { 
			distance += 4;
			target += 4;
			if (random.Next() % 5 == 0) target += 1;
			if (random.Next() % 5 == 4) { 
					target -= 1;
					bearing += 10;
			}
			update = Time();
		}
		if (Time () > 1000) {
			position = new Position((float)(51.400+Math.Cos(bearing*Math.PI/180)*distance/111229d), (float)(-0.15+Math.Sin(bearing*Math.PI/180)*distance/111229d));
		}
	}
	
	public long DistanceBehindTarget() {
		return target;
	}
	
	public long Time() {
		return timer.ElapsedMilliseconds;
	}
	
	public long Distance() {
		return distance;
	}
	
	public int Calories() {
		return (int)(factor * weight * distance/1000);
	}
	
	public float Pace() {
		return 1.0f;
	}
	
	public Position Position() {
		return position;
	}	
	
	public float Bearing() {
		return bearing;
	}
}
