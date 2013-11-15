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
	}
	
	public override string ToString() {
		if (string.Equals(node["type"], "challenge")) {
			return "Challenge from " + node["from"] + ": " + node["taunt"];
		}
		
		return "Notification from " + node["from"];
	}
}

