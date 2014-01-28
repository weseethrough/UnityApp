using UnityEngine;
using System.Collections;

/// <summary>
/// Display condition.
/// Specify a key for a boolean value in the DataVault which determines whether the 
/// </summary>
public class DisplayCondition : UIComponentSettings {
	
	public string KeyForBoolean = "key";
	public bool VisibleByDefault = true;
	public bool InvertCondition = false;
	
	// Use this for initialization
	void Start () {
		if(KeyForBoolean.Equals("key"))
		{
			UnityEngine.Debug.LogWarning("Key string not set for boolean");	
		}
		DataVault.RegisterListner(this, KeyForBoolean);
		Apply();
	}
	
	// Update is called once per frame
	void Update () {
			
	}
	
	public override void Apply ()
	{
		bool bShouldShow = VisibleByDefault;
		try {
			 bShouldShow = (bool)DataVault.Get(KeyForBoolean);
		} catch (System.Exception e) 
		{
			UnityEngine.Debug.Log("DisplayCondition: Key " + KeyForBoolean + "not found in Datavault");	
		}
		UnityEngine.Debug.Log("DisplayCondition: " + KeyForBoolean + " changed to " + bShouldShow);
		
		//invert if necessary
		bShouldShow = InvertCondition? !bShouldShow : bShouldShow;
		
		gameObject.SetActive(bShouldShow);
	}
	
//	void OnGUI () {
//		try {
//			bool bShouldShow = (bool)DataVault.Get(KeyForBoolean);
//			
//			//invert if necessary
//			bShouldShow = InvertCondition? !bShouldShow : bShouldShow;
//			
//			gameObject.SetActive(bShouldShow);
//		} catch (System.Exception e) {
//			UnityEngine.Debug.LogError("Error retreving display condition \"" + KeyForBoolean + "\" for element \"" + gameObject.name + ". Setting true");
//			//set it to true
//			DataVault.Set(KeyForBoolean, VisibleByDefault);
//		}
//	}
	
}
