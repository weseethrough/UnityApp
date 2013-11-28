using UnityEngine;
using System.Collections.Generic;

public class LazyTrack : Track {
	
	private bool fetched = false;
	
	private string name = null;
	public override string trackName { 
		get {			
			Fetch();
			return name;
		}
		set {
			name = value;
		}
	}
	private List<Position> positions = null;
	public override List<Position> trackPositions { 
		get {
			Fetch();
			return positions;
		}
		set {
			positions = value;
		}
	}
	
	public LazyTrack(int device_id, int track_id) 
	{
		trackId = track_id;
		deviceId = device_id;
	}
	
	public void Fetch() {
		if (fetched) return;
		fetched = true;
		Track track = Platform.Instance.FetchTrack(deviceId, trackId);
		name = track.trackName;
		positions = track.trackPositions;
	}
}
