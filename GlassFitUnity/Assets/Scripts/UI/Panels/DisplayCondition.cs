using UnityEngine;
using System.Collections;

/// <summary>
/// Display condition.
/// Specify a key for a boolean value in the DataVault which determines whether the 
/// </summary>
public class DisplayCondition : MonoBehaviour {
	
	public string KeyForBoolean = "key";
	public bool VisibleByDefault = true;
	
	// Use this for initialization
	void Start () {
		if(KeyForBoolean.Equals("key"))
		{
			UnityEngine.Debug.LogWarning("Key string not set for boolean");	
		}
	
	}
	
	// Update is called once per frame
	void Update () {
			
	}
	
	void OnGUI () {
		try {
			bool bShouldShow = (bool)DataVault.Get(KeyForBoolean);
			gameObject.SetActive(bShouldShow);
		} catch (System.Exception e) {
			UnityEngine.Debug.LogError("Error retreving display condition \"" + KeyForBoolean + "\" for element \"" + gameObject.name + ". Setting true");
			//set it to true
			DataVault.Set(KeyForBoolean, VisibleByDefault);
		}
	}
}
