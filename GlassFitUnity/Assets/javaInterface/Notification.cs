using System;
using SimpleJSON;

public class Notification
{		
	private string id;
	private bool read;
	
	private JSONNode node;
	
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

