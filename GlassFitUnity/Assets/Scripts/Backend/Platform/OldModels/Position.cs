using System;
using UnityEngine;

public class Position {
	
	public float latitude { get; set; }
	public float longitude { get; set; }
	private float degreesToMetres = 111111.11f;
	
	public Position(float latitude, float longitude) {
		this.latitude = latitude;
		this.longitude = longitude;
	}
	
	public Vector3 ToXYZ() {
		float x = latitude * degreesToMetres;
		float y = 0f; //height
		float z = longitude * degreesToMetres * (float)Math.Cos (latitude);
		return new Vector3(x,y,z);
	}
}