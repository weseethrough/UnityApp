using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
	private static T _instance;
	private static object _lock = new object();
	
	public static T Instance {
		get {
			if(applicationIsQuitting) {
				UnityEngine.Debug.Log("Singleton: already destroyed on application quit - won't create again");
				return null;
			}
			lock(_lock) {
				if(_instance == null) {
					_instance = (T) FindObjectOfType(typeof(T));
					if(FindObjectsOfType(typeof(T)).Length > 1) {
						UnityEngine.Debug.Log("Singleton: there is more than one singleton");
						return _instance;
					}
					if(_instance == null) {
						GameObject singleton = new GameObject();
						_instance = singleton.AddComponent<T>();
						singleton.name = "(singleton) " + typeof(T).ToString();
						
						DontDestroyOnLoad(singleton);
					} else {
						UnityEngine.Debug.Log("Singleton: already exists!!");
					}
				}
				return _instance;
			}
		}
	}
	
	private static bool applicationIsQuitting = false;
	
	public void OnDestroy() {
		applicationIsQuitting = true;
	}
}
