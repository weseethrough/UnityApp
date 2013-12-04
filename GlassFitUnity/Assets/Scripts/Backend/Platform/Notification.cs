using System;
using SimpleJSON;
using UnityEngine;

public class Notification
{		
	public string id { get; protected set; }
	public bool read { get; protected set; }
	
	public JSONNode node { get; protected set; }
	
	public AndroidJavaObject ajo { protected get; set; }
	
	public Notification ()
	{
	}
	public Notification (string id, bool read, string json) 
	{
		this.id = id;
		this.read = read;
		// Notification structure not standardized. Build subtypes using factory or duck type?
		this.node = JSON.Parse(json);		
		this.ajo = null;
	}
	
	public void setRead(bool read) {
		if (ajo == null) {
			Debug.Log ("Notification: attempted to setRead on notification without AndroidJavaObject");
			return;
		}
		ajo.Call("setRead", read);
		ajo.Call<int>("save");
		this.read = read;
	}
	
	public override string ToString() {
		if (string.Equals(node["type"], "challenge")) {
			return "Challenge from " + node["from"] + ": " + node["taunt"];
		}
		
		return "Notification from " + node["from"];
	}
}

