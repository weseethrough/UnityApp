using System;
using SimpleJSON;

public class Challenge
{	
	private Nullable<int> creatorId = null;
	private Nullable<DateTime> startTime = null;
	private Nullable<DateTime> stopTime = null;
	// location: GeoJSON?
	// attempts[]: track foreign keys
	private bool isPublic;
	
	public Challenge (string json) : this(JSON.Parse(json))
	{
	}
	public Challenge (JSONNode node) {
	}
}

