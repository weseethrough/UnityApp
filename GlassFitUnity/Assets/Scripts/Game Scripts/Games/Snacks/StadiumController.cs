using UnityEngine;
using System.Collections;

public class StadiumController : MonoBehaviour {
	
	private bool shouldMove;
	
	// Use this for initialization
	void Start () {
	
	}
	
	public void ShouldMove(bool move)
	{
		shouldMove = move;
	}
	
	// Update is called once per frame
	void Update () {
		if(shouldMove)
		{
			Vector3 currentPosition = transform.position;
			
			currentPosition.z -= Platform.Instance.Pace() * Time.deltaTime;
			
			transform.position = currentPosition;
		}
	}
}
