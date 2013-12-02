using System;
using SimpleJSON;

public abstract class Challenge
{	
	public string id { get; private set; }
	private Nullable<int> creatorId = null;
	private Nullable<DateTime> startTime = null;
	private Nullable<DateTime> stopTime = null;
	// location: GeoJSON?
	private JSONArray attempts = new JSONArray(); // track foreign keys
	public bool isPublic { get; private set; }
	
	public Challenge (string json) : this(JSON.Parse(json))
	{
	}
	public Challenge (JSONNode node) {
		if (node["_id"] != null) id = node["_id"].ToString();
		if (node["creator_id"] == null || String.Equals(node["creator_id"], "null")) creatorId = null;
		else creatorId = node["creator_id"].AsInt;
		if (node["public"] != null) isPublic = node["public"].AsBool;
		else isPublic = false;
		if (node["attempts"] != null) attempts = node["attempts"].AsArray;
	}
	
	public virtual JSONNode ToJson() {
		JSONNode node = new JSONClass();
		node["public"].AsBool = isPublic;
		return node;
	}
	
	public static Challenge Build(string json) {
		return Build (JSON.Parse(json));
	}
	public static Challenge Build(JSONNode node) {
		switch(node["type"]) {
		case "duration":
			return new DurationChallenge(node);
		case "distance":
			return new DistanceChallenge(node);
		case "pace":
			return new PaceChallenge(node);
		default:
			throw new NotImplementedException("Unknown challenge type: " + node["type"]);
		}
	}
	
	public Track CreatorTrack() {
		if (!creatorId.HasValue) return null;
		foreach (JSONNode attempt in attempts.Childs) {
			if (attempt["user_id"].AsInt == creatorId.Value) {
				return new LazyTrack(attempt["device_id"].AsInt, attempt["track_id"].AsInt);
			}
		}
		return null;
	}
	
	public Track LeaderTrack() {
		// TODO: Implement
		return LatestAttempt();
	}
	
	public Track UserTrack(int userId) {
		foreach (JSONNode attempt in attempts.Childs) {
			if (attempt["user_id"].AsInt == creatorId.Value) {
				return new LazyTrack(attempt["device_id"].AsInt, attempt["track_id"].AsInt);
			}
		}
		return null;
	}	

	public Track LatestAttempt() {
		if (attempts.Count == 0) return null;
		JSONNode attempt = attempts[0];
		// TODO: Sort attempts?
		return new LazyTrack(attempt["device_id"].AsInt, attempt["track_id"].AsInt);
	}
}

