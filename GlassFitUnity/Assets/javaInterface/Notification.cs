using System;
using SimpleJSON;

public class Notification
{		
	public string id { get; set; }
	public bool read { get; set; }
	
	public JSONNode node { get; set; }
	
	public Notification ()
	{
	}
	public Notification (string id, bool read, string json) 
	{
		this.id = id;
		this.read = read;
		// Notification structure not standardized. Build subtypes using factory or duck type?
		this.node = JSON.Parse(json);
		
		// DEBUG
		if (String.Equals(node["type"], "challenge")) {
			UnityEngine.Debug.Log("You have been challenged by " + node["from"] + " with: " + node["taunt"]);
			UnityEngine.Debug.Log(Challenge.Build((JSONNode)node["challenge"].AsObject).ToJson().ToString());
		}
	}
}

