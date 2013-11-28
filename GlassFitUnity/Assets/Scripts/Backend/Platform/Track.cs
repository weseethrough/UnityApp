using UnityEngine;
using System.Collections.Generic;

public class Track {
	
	public virtual string trackName { get; set; }
	public int trackId { get; set; }
	public int deviceId { get; set; }
	public virtual List<Position> trackPositions { get; set; }
	
	public Track() {}
	
	public Track(string name, int device_id, int track_id, List<Position> positions) 
	{
		trackName = name;
		trackId = track_id;
		deviceId = device_id;
		trackPositions = positions;
	}
}
