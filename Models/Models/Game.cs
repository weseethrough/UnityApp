using System;
using UnityEngine;

namespace RaceYourself.Models
{
	public class Game
	{
		public string gameId;// Unique identifier of the game (e.g. "Zombies 2")
		public string name; // Pretty name to display to users
		public string iconName;
		public string activity;
		public string description; // Pretty description to display to users
		public string state; // "Locked" or "Unlocked"
		public int tier; // which tier the game sits in (1,2,3,4 etc)
		public long priceInPoints;
		public long priceInGems;
		public string type;
		public int column;
		public int row;
		public string sceneName;

		public Game () {}
		public Game(string gameID, string name, string iconName, string activity, string description, 
			        string state, int tier, long priceInPoints, long priceInGems, string type, int column, 
			        int row, string sceneName)
		{
			this.gameId = gameID;
			this.name = name;
			this.iconName = iconName;
			this.activity = activity;
			this.description = description;
			this.state = state;
			this.tier = tier;
			this.priceInPoints = priceInPoints;
			this.priceInGems = priceInGems;
			this.type = type;
			this.column = column;
			this.row = row;
			this.sceneName = sceneName;
		}

		/// <summary>
		/// Unlock this game.
		/// </summary>
		public virtual void Unlock (/*Siaqodb db*/)
		{
			// TODO: PlayerPoints.payForGame(this) here or in Platform.UnlockGame()?
			PlayerPrefs.SetInt(this.gameId, 1);
			this.state = "unlocked";
		}

	}
}

