using System;
using SimpleJSON;

public class Friend
{
	private string name;
	private string uid;
	private string image;
	private bool hasGlass;
	private int userId;
		
	public Friend ()
	{
	}
	public Friend (string json) 
	{
		var node = JSON.Parse(json);
		name = node["name"];
		uid = node["uid"];
		image = node["image"];
		hasGlass = node["has_glass"].AsBool;
		userId = node["user_id"].AsInt;
		// Possibly provider-specific fields
	}
}

