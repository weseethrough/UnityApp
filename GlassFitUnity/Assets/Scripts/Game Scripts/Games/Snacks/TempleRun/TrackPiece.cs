using UnityEngine;
using System.Collections;

public class TrackPiece : MonoBehaviour {

	public Vector3 worldPos;
	protected Vector3 initialPos;
	
	private PlayerController player;

	public float GetDistance()
	{
		return worldPos.z;
	}
	public void SetDistance(float dist)
	{
		worldPos = new Vector3(initialPos.x, initialPos.y, dist + initialPos.z);
	}
	
	public void SetPlayer(PlayerController play)
	{
		player = play;
	}
	// Use this for initialization
	void Start () {
		//store the initial position as placed in the editor.
		initialPos = transform.position;
	}

	// Update is called once per frame
	void Update () {
		//move into position based on real-world position and player distance
		if(player != null) {
			transform.position = worldPos - new Vector3(0,0,(float)player.distanceTravelled);
		}
		
	}
}
