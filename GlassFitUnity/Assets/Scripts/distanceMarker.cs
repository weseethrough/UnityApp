using UnityEngine;
using System.Collections;

public class distanceMarker : MonoBehaviour {
	
	private int target = 50;
	
	//GameObjects for each marker
	public GameObject fiftyMarker;
	public GameObject fiftyMarker2;
	
	// 3DText boxes 
	public GameObject text1;
	public GameObject text2;
	
	// Text Meshes for 3D Text
	private TextMesh t1;
	private TextMesh t2;

	// Boxes to check distance
	private double distance;
	
	void Start () 
	{	
		// Get initial components
		t1 = text1.GetComponent<TextMesh>();
		t2 = text2.GetComponent<TextMesh>();
	}
	
	void ResetMarkers()
	{
		// Resets markers out of camera's view
		fiftyMarker.transform.position = new Vector3(0, 0, 500000);
		fiftyMarker2.transform.position = new Vector3(0, 0, 500000);
	}
	
	void Update () 
	{
		// Poll platform and set the new distance
		Platform.Instance.Poll();
		distance = Platform.Instance.Distance();
		
		// Reset markers
		ResetMarkers();
		
		// If markers within range, set new position
		if(distance > target - 20 && distance < target + 20)
		{
			double deltDist = target - distance;
			deltDist *= 6.666f;
			fiftyMarker.transform.position = new Vector3(15, 0, (float)deltDist);
			fiftyMarker2.transform.position = new Vector3(-15, 0, (float)deltDist);
		}
		
		// If current distance is higher than target set the new text
		if(distance > target + 20) 
		{
			target +=50;
			t1.text = target.ToString() + "m";
			t2.text = target.ToString() + "m";
		}
	}
}
