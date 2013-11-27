using UnityEngine;
using System.Collections;

/// <summary>
/// Sets the distance markers
/// </summary>
public class DistanceMarker : MonoBehaviour {
	
	private int target = 500;
	
	// 3DText box. 
	public GameObject textObject;
	
	// Text Mesh for 3D Text.
	private TextMesh textMesh;

	// Variable for current distance travelled.
	private double distance;
	
	/// <summary>
	/// Obtains the text mesh
	/// </summary>
	void Start () 
	{	
		// Get initial text mesh component.
		textMesh = textObject.GetComponent<TextMesh>();
	}
	
	/// <summary>
	/// Updates the position of the markers
	/// </summary>
	void Update () 
	{
		// Get current distance travelled.
		distance = Platform.Instance.Distance();
		
		// Reset markers.
		transform.position = new Vector3(0, 0, 500000);
		
		// If markers within range, set new position.
		if(distance > target - 50 && distance < target + 50)
		{
			double deltDist = target - distance;
			deltDist *= 135f;
			transform.position = new Vector3(-582, -109, (float)deltDist);
		}
		
		// If current distance is higher than target set the new text and position.
		if(distance > target + 50) 
		{
			target +=500;
			textMesh.text = SiDistance(target);
		}
	}
	
	/// <summary>
	/// Convert the distance text to the correct format.
	/// </summary>
	/// <returns>
	/// The distance in either meters or kilometers
	/// </returns>
	/// <param name='meters'>
	/// The start value in meters
	/// </param>
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