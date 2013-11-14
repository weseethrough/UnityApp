using System;
using SimpleJSON;


public class DurationChallenge : Challenge
{
	public int duration { get; set; }
	public int distance { get; set; }
	
	public DurationChallenge (string json) : this(JSON.Parse(json)) 
	{		
	}
	
	public DurationChallenge (JSONNode node) : base(node)
	{
		duration = node["duration"].AsInt;
		distance = node["distance"].AsInt;
	}

	public override JSONNode ToJson() {
		JSONNode node = base.ToJson();
		node["type"] = "duration";
		node["duration"].AsInt = duration;
		node["distance"].AsInt = distance;
		return node;
	}
}

