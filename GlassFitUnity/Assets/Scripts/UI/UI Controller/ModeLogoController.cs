using UnityEngine;
using System.Collections;

public class ModeLogoController : UIComponentSettings {

	// Use this for initialization
	void Start () {
		//subscribe to the key in the datavault
		DataVault.RegisterListner(this, "game_type_spritename");
		Apply();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public override void Apply () {
		//get the latest game type and pick the appropriate logo name
		string sName = (string)DataVault.Get("game_type_spritename");
		
		UISprite sprite = gameObject.GetComponent<UISprite>();
		sprite.spriteName = sName;
		
		UnityEngine.Debug.Log("Game Intro Panel: set logo sprite to " + sName);
	}

}
