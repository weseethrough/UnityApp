using UnityEngine;
using System.Collections;
using System;

public class DanselController : MonoBehaviour {
	protected float height;
	protected float xOffset;
	protected float finish;
	
	// Use this for initialization
	void Start () {
			//ascertain finish distance
			try {
			Track selectedTrack = (Track)DataVault.Get("current_track");
			if(selectedTrack != null) {
				finish = (int)selectedTrack.distance;
			} else {
				finish = (int)DataVault.Get("finish");
			}
		} catch(Exception e) {
			finish = 5000;	
		}
		xOffset = transform.localPosition.x;
		height = transform.localPosition.y;
	}
	
	// Update is called once per frame
	void Update () {
		float playerDistance = Platform.Instance.GetDistance();
		//position us at the finish, relative to player
		transform.localPosition = new Vector3(xOffset, height, finish-playerDistance);
	}
}
