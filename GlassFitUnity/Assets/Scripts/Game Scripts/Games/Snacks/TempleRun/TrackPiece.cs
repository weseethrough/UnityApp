using UnityEngine;
using System.Collections;

public class TrackPiece : MonoBehaviour {

	public Vector3 worldPos;
	//protected Vector3 initialPos;

	public float GetDistance()
	{
		return worldPos.z;
	}
	public void SetDistance(float dist)
	{
		worldPos = new Vector3(0, 0, dist);
	}

	// Use this for initialization
	void Start () {
		//store the initial position as placed in the editor.
		//initialPos = transform.position;
	}

	// Update is called once per frame
	void Update () {
		//move into position based on real-world position and player distance
		transform.position = worldPos - new Vector3(0,0,(float)Platform.Instance.LocalPlayerPosition.Distance);
	}
}
