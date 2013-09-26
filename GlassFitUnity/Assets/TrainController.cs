using UnityEngine;
using System.Collections;

public class TrainController : MonoBehaviour {
	
	private float countTime = 3.99f;
	private float whistleTime = 0.0f;
	private bool started = false;
	private double scaledDistance;
	private Platform inputData = null;
	private AudioSource trainMove;
	private AudioSource trainWhistle;
	
	// Use this for initialization
	void Start () {
		inputData = new Platform();
		var aSources = GetComponents<AudioSource>();
		trainMove = aSources[0];
		trainWhistle = aSources[1];
		
		trainMove.Play();
	}
	
	// Update is called once per frame
	void Update () {
		
		if(countTime == 3.99f && inputData.hasLock() && !started)
		{
			started = true;
		}
		
		if(started && countTime <= 0.0f)
		{
			inputData.StartTrack(false);
		}
		else if(started && countTime > 0.0f)
		{
			countTime -= Time.deltaTime;
		}
		
		whistleTime += Time.deltaTime;
		if(whistleTime >= 10.0f)
		{
			trainWhistle.Play();
			whistleTime -= 10.0f;
		}
		
		inputData.Poll();
	
		scaledDistance = (inputData.DistanceBehindTarget() - 50 * 6.666f);
		Vector3 movement = new Vector3(13.5f,-6.6f,(float)scaledDistance);
		transform.position = movement;
	}
}
