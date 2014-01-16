using UnityEngine;
using System.Collections;


/// <summary>
/// Junction. Represents a junction and two possible routes to the next junction: One straight forward, one via a detour.
/// Hooks up to a spline controller in the editor, which defines the detour path.
/// Reports how long its distance is on the currently selected path.
/// 
/// Until the spline is implemented, the detour path will send the train off at 60degrees for a time then back straight, then 60 degrees again.
/// </summary>
public class TrainTrackJunction : MonoBehaviour {
	public float distancePosition = 200.0f;	//how far into the run we are
	protected TrainController_Rescue trainComponent;
	public GameObject train;
	
	protected bool switched = false;
	protected bool playerHasPassed = false;
	protected bool trainHasPassed = false;
	
	protected float leverTriggerRange = 30.0f;
	
	public GameObject lever = null;
	
	float xOffset = 0.0f;
	float height = 0.0f;
	
	// Use this for initialization
	void Start () {
		//distancePosition = transform.position.z;
		xOffset = transform.position.x;
		height = transform.position.y;
		
		if(train != null)
		{
			trainComponent = train.GetComponent<TrainController_Rescue>();
		}
	}
	
	public void setTrain(GameObject trainObject)
	{
		trainComponent = trainObject.GetComponent<TrainController_Rescue>();	
	}
	
	public void SwitchOnDetour()
	{
		if(!switched)
		{
			switched = true;
			//flip animation
			//lever.transform.localRotation.z = lever.transform.localRotation.z * -1.0f;
			Vector3 rotation = lever.transform.localEulerAngles;
			rotation.z = rotation.z * -1.0f;
			lever.transform.localEulerAngles = rotation;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		//check if player has reached us
		if(!playerHasPassed)
		{
			if(Platform.Instance.GetDistance() + leverTriggerRange > distancePosition && !trainHasPassed)
			{
				SwitchOnDetour();
				//play sound
				AudioSource source = (AudioSource)GetComponent(typeof(AudioSource));
				source.Play();
			}
		}
		
		//check if train has reached us
		if(!trainHasPassed)
		{
			if(trainComponent.GetForwardDistance() > distancePosition)
			{
				trainHasPassed = true;
				if(switched)
				{
					trainComponent.BeginDetour();
				}
			}
		}
		
		//set position relative to player
		transform.localPosition = new Vector3(xOffset, height, distancePosition - Platform.Instance.GetDistance());
	}

}
