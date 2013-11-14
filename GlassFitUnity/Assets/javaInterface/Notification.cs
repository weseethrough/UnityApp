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
		UnityEngine.Debug.Log ("Platform: " + json);
		this.node = JSON.Parse(json);
	}
}

