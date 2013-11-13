using UnityEngine;
using System.Collections;
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
	}
	
	public void StartTrack(bool indoor) {
		throw new NotImplementedException();
	}
	
	public Boolean hasLock() {
		return true;
	}
	
	public void stopTrack() {
		throw new NotImplementedException();
	}
	
	public void reset() {
		throw new NotImplementedException();
	}
	
	public void setTargetSpeed(float speed)
	{
		throw new NotImplementedException();
	}
	
	public void setTargetTrack(int trackID)
	{
		throw new NotImplementedException();
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
