using UnityEngine;
using System.Collections;
using System;

public class DanselController : MonoBehaviour {
	protected float height;
	protected float xOffset;
	protected float zPosition;
	
	protected float zOffset = 10.0f;
	// Use this for initialization
	void Start () {
		zPosition = 350;
		
		xOffset = transform.localPosition.x;
		height = transform.localPosition.y;
		
		
	}
	
	// Update is called once per frame
	void Update () {
		float playerDistance = (float)Platform.Instance.LocalPlayerPosition.Distance;
		
		zPosition -= Platform.Instance.LocalPlayerPosition.Pace * Time.deltaTime;
		
		//position us at the finish, relative to player
		transform.localPosition = new Vector3(xOffset, height, zPosition+zOffset);
	}
}
