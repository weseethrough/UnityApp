using System;
using SimpleJSON;

public class Friend
{
	public string name { get; protected set; }
	public string uid { get; protected set; }
	public string image { get; protected set; }
	public bool hasGlass { get; protected set; }
	public string provider { get; protected set; }
	public Nullable<int> userId { get; protected set; }
	public string guid { get; protected set; }
		
	public Friend (string json) 
	{
		var node = JSON.Parse(json);
		name = node["name"];
		uid = node["uid"];
		image = node["photo"];
		provider = node["provider"];
		hasGlass = node["has_glass"].AsBool;
		if (String.Equals(node["user_id"], "null")) userId = null;
		else userId = node["user_id"].AsInt;
		guid = node["_id"];
		// Possibly provider-specific fields
	}
}

