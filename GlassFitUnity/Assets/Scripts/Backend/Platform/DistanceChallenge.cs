using System;
using SimpleJSON;


public class DistanceChallenge : Challenge
{
	public int distance { get; set; }
	public int time { get; set; }
	
	public DistanceChallenge (string json) : this(JSON.Parse(json)) 
	{		
	}
	
	public DistanceChallenge (JSONNode node) : base(node)
	{
		distance = node["distance"].AsInt;
		time = node["time"].AsInt;
	}

	public JSONNode ToJson() {
		JSONNode node = base.ToJson();
		node["type"] = "distance";
		node["distance"].AsInt = distance;
		node["time"].AsInt = time;
		return node;
	}
}

