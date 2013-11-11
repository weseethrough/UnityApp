using UnityEngine;
using System.Collections;

public class TrainSound : MonoBehaviour {

	private float whistleTime = 0.0f;
	private AudioSource trainMove;
	private AudioSource trainWhistle;
	private bool movePlaying;
	public GameObject train;
	
	// Use this for initialization
	void Start () {
		var aSources = GetComponents<AudioSource>();
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
			
			transform.position = new Vector3(0, 0, (float)Platform.Instance.DistanceBehindTarget()-50.0f);
		}
		else {
			movePlaying = false;
			trainMove.Stop();
		}
	}
}
