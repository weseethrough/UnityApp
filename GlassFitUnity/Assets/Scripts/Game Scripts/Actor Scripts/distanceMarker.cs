using UnityEngine;
using System.Collections;

public class DistanceMarker : MonoBehaviour {
	
	private int target = 500;
	
	// 3DText boxes 
	public GameObject textObject;
	
	// Text Meshes for 3D Text
	private TextMesh textMesh;

	// Boxes to check distance
	private double distance;
	
	void Start () 
	{	
		// Get initial components
		textMesh = textObject.GetComponent<TextMesh>();
	}
	
	void Update () 
	{
		distance = Platform.Instance.Distance();
		
		// Reset markers
		transform.position = new Vector3(0, 0, 500000);
		
		// If markers within range, set new position
		if(distance > target - 50 && distance < target + 50)
		{
			double deltDist = target - distance;
			deltDist *= 135f;
			transform.position = new Vector3(-582, -109, (float)deltDist);
		}
		
		// If current distance is higher than target set the new text
		if(distance > target + 50) 
		{
			target +=500;
			textMesh.text = SiDistance(target);
		}
	}
	
	string SiDistance(double meters) {
		string postfix = "m";
		string final;
		float value = (float)meters;
		if (value > 1000) {
			value = value/1000;
			postfix = "km";
			final = value.ToString("f1");
		}
		else
		{
			final = value.ToString("f0");
		}
		return final+postfix;
	}
}
