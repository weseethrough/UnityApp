using UnityEngine;
using System.Collections.Generic;
using System;

public class Game {
	
	private AndroidJavaObject javaGame;  // reference to JNI object so we can call methods on it
	public string gameId { get; set; } // Unique identifier of the game (e.g. "Zombies 2")
	public string name { get; set; } // Pretty name to display to users
	public string activity { get; set; }

	public string description { get; set; } // Pretty description to display to users
	public string state { get; set; } // "Locked" or "Unlocked"
	public int tier { get; set; } // which tier the game sits in (1,2,3,4 etc)
	public long priceInPoints { get; set; }

	public long priceInGems { get; set; }
	
	public Game () {
	}
	
	public void initialise (AndroidJavaObject javaGame) {
		this.javaGame = javaGame;
		try {
			gameId = javaGame.Call<string> ("getGameId");
			name = javaGame.Call<string> ("getName");
			activity = javaGame.Call<string> ("getActivity");
			description = javaGame.Call<string> ("getDescription");
			state = javaGame.Call<string> ("getState");
			tier = javaGame.Call<int> ("getTier");
			priceInPoints = javaGame.Call<long> ("getPriceInPoints");
			priceInGems = javaGame.Call<long> ("getPriceInGems");
			UnityEngine.Debug.Log ("Game: Successfuly imported game: " + gameId);
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning ("Game: Error importing game: " + gameId);
			UnityEngine.Debug.LogException (e);
		}
	}
	
	public void unlock () {
		try {
			AndroidJavaObject updatedJavaGame = javaGame.Call<AndroidJavaObject> ("unlock");
			this.initialise (updatedJavaGame);
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning ("Game: Error unlocking game: " + gameId);
			UnityEngine.Debug.LogException (e);
		}
	}
	
}
