using System;
using SimpleJSON;

public abstract class Challenge
{	
	private Nullable<int> creatorId = null;
	private Nullable<DateTime> startTime = null;
	private Nullable<DateTime> stopTime = null;
	// location: GeoJSON?
	// attempts[]: track foreign keys
	private bool isPublic = false;
	
	public Challenge (string json) : this(JSON.Parse(json))
	{
	}
	public Challenge (JSONNode node) {
		if (node["creator_id"] == null || String.Equals(node["creator_id"], "null")) creatorId = null;
		else creatorId = node["creator_id"].AsInt;
		if (node["public"] != null) isPublic = node["public"].AsBool;
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
}

