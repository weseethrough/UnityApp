<<<<<<< HEAD
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.IO;

[ExecuteInEditMode()] 
public class PlatformDummy : MonoBehaviour 
{
	
	private Stopwatch timer = new Stopwatch();
	private System.Random random = new System.Random();
	private long update = 0;
	private float distance = 0;
	private float target = 1;
	private float targetSpeed = 4;
	private Position position = null;
	private float bearing = 45; // degrees	
	
	private const float weight = 75.0f;
	private const float factor = 1.2f;
	
	private string blobstore = "editor-blobstore";
	private string blobassets = "blob";
	
	private static PlatformDummy _instance;
	private bool initialised = false;	
	private List<Game> games;
	
	public static PlatformDummy Instance 
    {
		get 
        {
            if (_instance == null)
            {
                PlatformDummy[] pDummies = FindObjectsOfType(typeof(PlatformDummy)) as PlatformDummy[];
                int count = pDummies.Length;
                if (count >= 1)
                {
                    if (count > 1)
                    {
                        UnityEngine.Debug.Log("Singleton: there is more than one singleton");
                    }

                    for (int i = 1; i < count; i++)
                    {
                        GameObject plat = GameObject.Find("PlatformDummy");
                        Destroy(plat);
                    }

                    _instance = FindObjectOfType(typeof(PlatformDummy)) as PlatformDummy;
                }

                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    _instance = singleton.AddComponent<PlatformDummy>();
                    singleton.name = "PlatformDummy"; // Used as target for messages

                    DontDestroyOnLoad(singleton);
                }                
            }
			
			if (_instance != null)
			{
				if (_instance.initialised == false)
				{
					_instance.Initialize();
				}
			}
            return _instance;
		}
	}
	
	private static bool applicationIsQuitting = false;
	
	public void OnDestroy() 
	{
		applicationIsQuitting = true;
	}
	
	void Awake() 
	{
		if (initialised == false)
		{
			Initialize();
		}
	}
	
	void Initialize()
	{
		//timer.Start();
		blobstore = Path.Combine(Application.persistentDataPath, blobstore);
		Directory.CreateDirectory(blobstore);
		blobassets = Path.Combine(Application.streamingAssetsPath, blobassets);
		Directory.CreateDirectory(blobassets);
		UnityEngine.Debug.Log("Editor blobstore: " + blobstore + ", blobassets: " + blobassets);
		
		games = new List<Game>(9);
		
		Game current = new Game("Race Yourself (run)", "activity_run","run", "Run against an avatar that follows your previous track","unlocked",1,0,0, "Race", 0, 0);
		games.Add(current);
		
		current = new Game("Switch to cycle mode (run)","activity_bike","run","Switch to cycle mode","locked",1,1000,0, "Race", 1, 0);
        games.Add(current);
		
		current = new Game("Zombies 1","activity_zombie","run","We all want to see if we could survive the zombie apocalypse, and now you can! Remember the #1 rule - cardio.","locked",2,50000,0, "Pursuit", 0, -1);
        games.Add(current);
		
		current = new Game("Boulder 1","activity_boulder","run","Relive that classic moment in Indiana Jones, run from the boulder! No treasure this time though.","locked",1,10000,0, "Pursuit", -1, 0);
        games.Add(current);
		
		current = new Game("Dinosaur 1","activity_dinosaurs","run","Remember that time in Jurassic Park when the T-Rex ate those guys? Try to avoid the same fate!","locked",3,100000,0, "Pursuit", 0, 1);
        games.Add(current);
		
		current = new Game("Eagle 1","activity_eagle","run","You stole her eggs, now the giant eagle is after you! It's not your fault the eggs are really tasty...","locked",2,70000,0, "Pursuit", -1, 1);
        games.Add(current);
		
		current = new Game("Train 1","activity_train","run","Run away from a train!","locked",2,20000,0, "Pursuit", 1, 1);
		games.Add(current);
		
		current = new Game("Mo Farah","activity_farah","run","Run against Mo Farah! Try and beat his almost world record 10km!","unlocked",2,70000,0, "Celebrity", 2, 0);
        games.Add(current);
		
		current = new Game("Paula Radcliffe","activity_paula_radcliffe","run","Run a marathon with Paula Radcliffe! Try and beat her time at the 2007 NYC Marathon!","unlocked",2,20000,0, "Celebrity", 2, 1);
		games.Add(current);
		
		initialised = true;
	}
	
	public void StartTrack() {
		timer.Start();
	}
	
	public Boolean HasLock() {
		return true;
	}
	
	public void StopTrack() {
		timer.Stop();
	}
	
	public void Reset() {
		timer.Stop();
		timer.Reset();
		distance = 0;
		update = 0;
		target = 1;
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
			UnityEngine.Debug.Log("PlatformDummy: Trying id: " + id);
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
		//if (Time() - update > 1000) { 
			distance += 4f * UnityEngine.Time.deltaTime;
			target += targetSpeed * UnityEngine.Time.deltaTime;
			if (random.Next() % 5 == 0) target += 1 * UnityEngine.Time.deltaTime;
			if (random.Next() % 5 == 4) { 
					target -= 1 * UnityEngine.Time.deltaTime;
					bearing += 10;
			}
			update = Time();
		//}
		//if (Time () > 1000) {
			position = new Position((float)(51.400+Math.Cos(bearing*Math.PI/180)*distance/111229d), (float)(-0.15+Math.Sin(bearing*Math.PI/180)*distance/111229d));
		//}
	}
	
	public float GetTargetSpeed() {
		return targetSpeed;
	}
	
	public float DistanceBehindTarget() {
		return target - distance;
	}
	
	public long Time() {
		return timer.ElapsedMilliseconds;
	}
	
	public float Distance() {
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
	
	public List<Game> GetGames() {

		return games;
	}
}
=======
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.IO;

public class PlatformDummy : MonoBehaviour {
	
	private Stopwatch timer = new Stopwatch();
	private System.Random random = new System.Random();
	private long update = 0;
	private float distance = 0;
	private float target = 1;
	private float targetSpeed = 4;
	private Position position = null;
	private float bearing = 45; // degrees	
	
	private const float weight = 75.0f;
	private const float factor = 1.2f;
	
	private string blobstore = "editor-blobstore";
	private string blobassets = "blob";
	
	private static PlatformDummy _instance;
	private bool initialised = false;
	private static object _lock = new object();
	private List<Game> games;
	
	public static PlatformDummy Instance {
		get {
//			if(applicationIsQuitting) {
//				UnityEngine.Debug.Log("Singleton: already destroyed on application quit - won't create again");
//				return null;
//			}
			lock(_lock) {
				if(_instance == null) {
					_instance = (PlatformDummy) FindObjectOfType(typeof(PlatformDummy));
					if(FindObjectsOfType(typeof(PlatformDummy)).Length >= 1) {
						for(int i=0; i < FindObjectsOfType(typeof(PlatformDummy)).Length; i++)
						{
							GameObject plat = GameObject.Find("PlatformDummy");
							Destroy(plat);
						}
						UnityEngine.Debug.Log("Singleton: there is more than one singleton");
						_instance = null; 
						//return _instance;
					}
					if(_instance == null) {
						GameObject singleton = new GameObject();
						_instance = singleton.AddComponent<PlatformDummy>();
						singleton.name = "PlatformDummy"; // Used as target for messages
						
						DontDestroyOnLoad(singleton);
					} else {
						UnityEngine.Debug.Log("Singleton: already exists!!");
					}
				}
				while(!_instance.initialised) {
					continue;
				}
					return _instance;
			}
		}
	}
	
	private static bool applicationIsQuitting = false;
	
	public void OnDestroy() {
		applicationIsQuitting = true;
	}
	
	protected PlatformDummy() {
		//timer.Start();
		blobstore = Path.Combine(Application.persistentDataPath, blobstore);
		Directory.CreateDirectory(blobstore);
		blobassets = Path.Combine(Application.streamingAssetsPath, blobassets);
		Directory.CreateDirectory(blobassets);
		UnityEngine.Debug.Log("Editor blobstore: " + blobstore + ", blobassets: " + blobassets);
		
		games = new List<Game>(9);
		
		Game current = new Game("Race Yourself (run)", "activity_run","run", "Run against an avatar that follows your previous track","unlocked",1,0,0, "Race", 0, 0);
		games.Add(current);
		
		current = new Game("Switch to cycle mode (run)","activity_bike","run","Switch to cycle mode","locked",1,1000,0, "Race", 1, 0);
        games.Add(current);
		
		current = new Game("Zombies 1","activity_zombie","run","We all want to see if we could survive the zombie apocalypse, and now you can! Remember the #1 rule - cardio.","locked",2,50000,0, "Pursuit", 0, -1);
        games.Add(current);
		
		current = new Game("Boulder 1","activity_boulder","run","Relive that classic moment in Indiana Jones, run from the boulder! No treasure this time though.","locked",1,10000,0, "Pursuit", -1, 0);
        games.Add(current);
		
		current = new Game("Dinosaur 1","activity_dinosaurs","run","Remember that time in Jurassic Park when the T-Rex ate those guys? Try to avoid the same fate!","locked",3,100000,0, "Pursuit", 0, 1);
        games.Add(current);
		
		current = new Game("Eagle 1","activity_eagle","run","You stole her eggs, now the giant eagle is after you! It's not your fault the eggs are really tasty...","locked",2,70000,0, "Pursuit", -1, 1);
        games.Add(current);
		
		current = new Game("Train 1","activity_train","run","Run away from a train!","locked",2,20000,0, "Pursuit", 1, 1);
		games.Add(current);
		
		current = new Game("Mo Farah","activity_farah","run","Run against Mo Farah! See how you compare to his 2012 Olympic time!","unlocked",2,70000,0, "Celebrity", 2, 0);
        games.Add(current);
		
		current = new Game("Paula Radcliffe","activity_paula_radcliffe","run","Run a marathon with Paula Radcliffe! Try and beat her time at the 2007 NYC Marathon!","unlocked",2,20000,0, "Celebrity", 2, 1);
		games.Add(current);
		
		current = new Game("Chris Hoy", "activity_chris_hoy", "run", "Cycle with Chris Hoy, in his almost record breaking 1km cycle in 2007", "unlocked", 2, 10000, 0, "Celebrity", 2, -1);
        games.Add(current);
		
		current = new Game("Bradley Wiggins", "activity_bradley_wiggins", "cycle", "Participate in a 4km pursuit race with Bradley Wiggins on his 2008 Olympics gold medal time", "unlocked", 2, 10000, 0, "Celebrity", 1, -1);
        games.Add(current);
		
		current = new Game("Fire", "activity_fire", "run", "Know what's good on a barbeque? Burgers. Know what isn't? You. So run before you get burned.", "unlocked", 2, 10000, 0, "Pursuit", 1, 2);
		games.Add(current);
		
		initialised = true;
	}
	
	public void StartTrack() {
		timer.Start();
	}
	
	public Boolean HasLock() {
		return true;
	}
	
	public void StopTrack() {
		timer.Stop();
	}
	
	public void Reset() {
		timer.Stop();
		timer.Reset();
		distance = 0;
		update = 0;
		target = 1;
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
			UnityEngine.Debug.Log("PlatformDummy: Trying id: " + id);
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
		//if (Time() - update > 1000) { 
			distance += 4f * UnityEngine.Time.deltaTime;
			target += targetSpeed * UnityEngine.Time.deltaTime;
			if (random.Next() % 5 == 0) target += 1 * UnityEngine.Time.deltaTime;
			if (random.Next() % 5 == 4) { 
					target -= 1 * UnityEngine.Time.deltaTime;
					bearing += 10;
			}
			update = Time();
		//}
		//if (Time () > 1000) {
			position = new Position((float)(51.400+Math.Cos(bearing*Math.PI/180)*distance/111229d), (float)(-0.15+Math.Sin(bearing*Math.PI/180)*distance/111229d));
		//}
	}
	
	public float GetTargetSpeed() {
		return targetSpeed;
	}
	
	public float DistanceBehindTarget() {
		return target - distance;
	}
	
	public long Time() {
		return timer.ElapsedMilliseconds;
	}
	
	public float Distance() {
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
	
	public List<Game> GetGames() {

		return games;
	}
}
>>>>>>> 73aed74758289118dc23931e31b8b9ff0c75e2fd
