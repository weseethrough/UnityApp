using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModeLogoController : UIComponentSettings {
	
	public string gameIDKey = "current_game_id";
	
	// Use this for initialization
	void Start () {
		//subscribe to the key in the datavault
		DataVault.RegisterListner(this, gameIDKey);
		Apply();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public override void Apply () {
		//get the latest game type and pick the appropriate logo name
		string sName = (string)DataVault.Get(gameIDKey);
		
		
		UnityEngine.Debug.Log("Game Intro Panel: set logo sprite to " + sName);
		
		//get the game title and description and update the relevant strings in the datavault
		List<Game> games = Platform.Instance.GetGames();
		
		Game currentGame = games.Find( r => r.gameId == sName );
		
		//set correct sprite
		UISprite sprite = gameObject.GetComponent<UISprite>();
		sprite.spriteName = currentGame.iconName;
		
		//set title and description
		DataVault.Set("game_type_title", currentGame.name);
		DataVault.Set("game_type_subtitle", currentGame.description);
		
	}

}
