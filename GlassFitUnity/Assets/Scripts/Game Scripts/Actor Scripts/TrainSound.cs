using UnityEngine;
using System.Collections;

public class TrainSound : MonoBehaviour {

	private float whistleTime = 0.0f;
	private AudioSource trainMove;
	private AudioSource trainWhistle;
	private bool movePlaying;
	private GameObject train;
	private TargetController controller;
	
	// Use this for initialization
	void Start () {
		var aSources = GetComponents<AudioSource>();
		train = transform.parent.gameObject;
		controller = train.GetComponent<TargetController>();
		trainMove = aSources[0];
		trainWhistle = aSources[1];
		
		if(train.activeSelf) {
			trainMove.Play();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(train.activeSelf) {
			whistleTime += Time.deltaTime;
			
			if(!movePlaying) {
				trainMove.Play();
				movePlaying = true;
			}
			
			if(whistleTime >= 10.0f)
			{
				trainWhistle.Play();
				whistleTime -= 10.0f;
			}
			
			transform.position = new Vector3(0, 0, (float)Platform.Instance.DistanceBehindTarget(controller.target)-50.0f);
		}
		else {
			movePlaying = false;
			trainMove.Stop();
		}
	}
}
