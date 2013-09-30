using UnityEngine;
using System.Collections;

public class TrainController : MonoBehaviour {
	
	private float countTime = 3.99f;
	private float trainTime = 0f;
	private float whistleTime = 0.0f;
	private bool started = false;
	private double scaledDistance;
	private Platform inputData = null;
	private AudioSource trainMove;
	private AudioSource trainWhistle;
	private float FakedMovement = -250f;
	// Use this for initialization
	void Start () {
		inputData = new Platform();
		var aSources = GetComponents<AudioSource>();
		trainMove = aSources[0];
		trainWhistle = aSources[1];
		
		trainMove.Play();
	}
	
	void OnEnable() {
		transform.position = new Vector3(10, -10, 50);
		//Debug.Log("OnEnable called\n\n\n\n\n");
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
		FakedMovement  += (Time.deltaTime)*6;
		if(whistleTime >= 10.0f)
		{
			trainWhistle.Play();
			whistleTime -= 10.0f;
		}
		
		inputData.Poll();
	
		scaledDistance = (inputData.DistanceBehindTarget() - 50 * 6.666f);

		scaledDistance = FakedMovement;
		Vector3 movement = new Vector3(13.5f,-6.6f,(float)scaledDistance);
		transform.position = movement;
	}
}
