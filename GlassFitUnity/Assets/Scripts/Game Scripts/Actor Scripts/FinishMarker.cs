using UnityEngine;
using System.Collections;

public class FinishMarker : MonoBehaviour {
	
	private int target;
	
	private double distance;
	
	// Use this for initialization
	void Start () {
		target = (int)DataVault.Get("finish") * 1000;
		transform.position = new Vector3(0f, -595f, 500000f);
	}
	
	// Update is called once per frame
	void Update () {
		distance = Platform.Instance.Distance();
		
		if(distance > target - 100) 
		{
			double deltDist = target - distance;
			deltDist *= 135f;
			transform.position = new Vector3(0, -595f, (float)deltDist);
		}
	}
}
