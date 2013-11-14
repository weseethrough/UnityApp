using System;
using SimpleJSON;


public class PaceChallenge : Challenge
{
	public int distance { get; set; }
	public int pace { get; set; }
	
	public PaceChallenge (string json) : this(JSON.Parse(json)) 
	{		
	}
	
	public PaceChallenge (JSONNode node) : base(node)
	{
		distance = node["distance"].AsInt;
		pace = node["pace"].AsInt;
	}

	public override JSONNode ToJson() {
		JSONNode node = base.ToJson();
		node["type"] = "pace";
		node["distance"].AsInt = distance;
		node["pace"].AsInt = pace;
		return node;
	}
}

