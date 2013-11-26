using System;
using SimpleJSON;

public class Friend
{
	public string name { get; set; }
	public string uid { get; set; }
	public string image { get; set; }
	public bool hasGlass { get; set; }
	public string provider { get; set; }
	public Nullable<int> userId { get; set; }
		
	public Friend ()
	{
	}
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
		// Possibly provider-specific fields
	}
}

