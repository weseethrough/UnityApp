using System;
using Newtonsoft.Json;
using Sqo.Attributes;
using UnityEngine;
using Sqo;

namespace RaceYourself.Models
{
	public class Game
	{
		// Game
        
        [Index]
        [UniqueConstraint]
        [JsonProperty("id")]
        public string gameId;// Unique identifier of the game (e.g. "Zombies 2")
        public string name; // Pretty name to display to users
        public string activity;
        public string description; // Pretty description to display to users
        public int tier; // which tier the game sits in (1,2,3,4 etc)
        [JsonProperty("price_in_points")]
        public long priceInPoints;
        [JsonProperty("price_in_gems")]
        public long priceInGems;
		public string type; // N/A, Race, Challenge, Snack
        [JsonProperty("scene_name")]
        public string sceneName;

        // Game state
        
        [JsonConverter(typeof(LockedConverter))]
        public string state; // "Locked" or "Unlocked"
        public bool enabled; // Can the user access this game? Server-driven; overrides player unlocks
        
        // Menu-specific

        [JsonProperty("icon")]
        public string iconName;
        public int column;
		public int row;

        // For server sync

        // Populated by server if game is to be deleted. (???)
        [JsonIgnore]
        public DateTime? deleted_at;
        [JsonIgnore]
        // is there a local change to this game that is awaiting a server sync?
        public bool dirty = false;

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

// TODO HAXXXX look into whether we can just make 'locked' a bool.
class LockedConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((bool)value) ? "Locked" : "Unlocked");
    }
    
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return reader.Value.ToString() == "Locked";
    }
    
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(bool) || objectType == typeof(string);
    }
}