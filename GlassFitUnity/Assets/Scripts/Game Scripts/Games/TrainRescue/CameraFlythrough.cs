using UnityEngine;
using System.Collections;
using System;


/// <summary>
/// Camera flythrough.
/// For now, this will just simplistically fly through to the damsel at the end, then back towards the start.
/// </summary>
public class CameraFlythrough : MonoBehaviour {

	protected enum FlythroughState {
		WaitingStart,
		Forward,
		WaitingEnd,
		Backward,
		ToTrain,
		ViewTrain,
		ToStartLine,
		Finished,
	}
	
	protected float parametricDist = 0.0f;
	protected FlythroughState state = FlythroughState.WaitingStart;
	protected float finish = 0.0f;
	
	//parametres determining the way the flythrough moves
	protected float parametricFlySpeed = 0.4f;
	protected float endWaitTimer = 0.0f;
	protected float endWaitDuration = 2.0f;
	protected float endOffsetDist = 0.0f;
	protected float heightDeltaMagnitude = 5.0f;
	
	protected float xOffset = 0.0f;
	protected float height = 0.0f;
	
	bool showSubtitleCard = false;
	bool hudReturn = false;
	bool bStartedRoutine = false;
	bool bStartedEndTurn = false;
	
	Quaternion camOrientation = Quaternion.identity;
	bool shouldUseCamOrientation = true;
	
	// Use this for initialization
	void Start () {
		//Get the end distance
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
		
		finish = 350;
		
		//make it so we stop short of the damsel herself.
		finish -= endOffsetDist;
		
		xOffset = transform.localPosition.x;
		height = transform.localPosition.y;
		
#if UNITY_EDITOR
		StartFlythrough();
#endif
			
	}
	
	public void StartFlythrough()
	{
		state = FlythroughState.Forward;
	}
	
	// Update is called once per frame
	void Update () {
		float distance = 0.0f;
		GameObject lookAtTarget = null;
		GameObject damsel = GameObject.Find("Damsel_Tracks");
		GameObject train = GameObject.Find("Train_Rescue");

		//if we're currently flying through, move towards the next part of the curve.
		switch(state)
		{
			case FlythroughState.WaitingStart:
			{
				//nothing to do
				return;
			}
			case FlythroughState.Finished:
			{
				//nothing to do
				return;
			}
			case FlythroughState.Forward:
			{
				//move towards end
				parametricDist += Time.deltaTime*parametricFlySpeed;
				//clamp at end
				if(parametricDist > 1.0f) 
				{
					parametricDist = 1.0f; 
					state = FlythroughState.WaitingEnd;
					UnityEngine.Debug.Log("Flythrough: readched endpoint");
				}
				//set distance
				distance = 	parametricDist * finish;
				lookAtTarget = damsel;
				break;
			}
			case FlythroughState.WaitingEnd:
			{
				if(!bStartedRoutine)
				{
					StartCoroutine(MidFlythroughSequence());
					bStartedRoutine = true;
				}
				else
				{
					//set the camera orientation here. Doing it from the coroutine is the wrong timing. Gets stomped by minimalsensorcam.
					if(shouldUseCamOrientation)
					{
						transform.rotation = camOrientation;
					}
				}
				transform.rotation = camOrientation;
				distance = parametricDist * finish;
				lookAtTarget = null;
				break;
			}
			case FlythroughState.Backward:
			{
				parametricDist -= Time.deltaTime * parametricFlySpeed;
				if(parametricDist < 0.0f) 
				{ 
					parametricDist = 0.0f; 
					state = FlythroughState.ViewTrain;

				}
				lookAtTarget = train;
				float offset = 0.0f;	//we'll be this far back from the train
				distance = parametricDist * (finish + offset) - offset;
				
				break;
			}
		case FlythroughState.ViewTrain:
		{
			//turn towards forward
			if(!bStartedEndTurn)
			{
				StartCoroutine( EndFlythroughSequence() );
				bStartedEndTurn = true;
			}
			else
			{
				//set the camera orientation here. Doing it from the coroutine is the wrong timing. Gets stomped by minimalsensorcam.
				if(shouldUseCamOrientation)
				{
					transform.rotation = camOrientation;
				}
			}
			break;
		}
		}
		
		//add lofted height delta
		//set transform. Player distance should be zero at this point.
		transform.localPosition = new Vector3(xOffset, height + getHeightDelta(), distance);
		
		//look at the damsel
		if(lookAtTarget != null)
		{
			Vector3 lookAtPos = lookAtTarget.transform.localPosition;
			//fudge downwards. Seems necessary for some reason;
			lookAtPos = lookAtPos + new Vector3(0,0,0);
			transform.LookAt(lookAtPos);
			//store this orientation
			camOrientation = transform.rotation;
		}
		else
		{
			//do nothing
		}
	}
	
	IEnumerator MidFlythroughSequence()
	{
		//wait a few seconds
		yield return new WaitForSeconds(1.0f);
		
		UnityEngine.Debug.Log("CameraFly: Changing to subtitle");
		
		//show the subtitle card for a second
		if(!hudReturn) {
			FollowFlowLinkNamed("Subtitle");		
		}
		
		DataVault.Set("train_subtitle", "\"Help\"\n\n\"Please Save Me!\"");
		yield return new WaitForSeconds(2.0f);
		
		//back to HUD
		FollowFlowLinkNamed("ToBlank");
		hudReturn = true;
		
		//reset the gyros here
//		MinimalSensorCamera cam = (MinimalSensorCamera)GameObject.FindObjectOfType(typeof(MinimalSensorCamera));
//		cam.ResetGyro();
		
		//wait another second on the maiden
		yield return new WaitForSeconds(1.0f);
		
		//turn around
		float updateInterval = 0.025f;
		GameObject train = GameObject.Find("Train_Rescue");
		GameObject lookerAtter = new GameObject();
		lookerAtter.transform.LookAt(train.transform.position);
		
		Quaternion source = camOrientation;
		Quaternion target = lookerAtter.transform.localRotation;
		
		float t = 0.0f;
		
		while(t < 1.0f)
		{
			if (t > 1.0f) { t = 1.0f; }
			camOrientation = Quaternion.Lerp(source, target, t);
			shouldUseCamOrientation = true;
			t+= 1.0f * updateInterval;
			yield return new WaitForSeconds(updateInterval);
		}
		//shouldUseCamOrientation = false;
		
		//move to next state of flythrough
		state = FlythroughState.Backward;
	}
	
	IEnumerator EndFlythroughSequence()
	{
		Quaternion forward = Quaternion.identity;
		GameObject train = GameObject.Find("Train_Rescue");
		transform.LookAt(train.transform.localPosition);
		Quaternion current = transform.localRotation;
		
		float t = 0;
		float interval = 0.025f;
		while(t<1.0f)
		{	
			if(t>1.0f) { t = 1.0f; }
			UnityEngine.Debug.Log("TrainCamera: T = " + t );
			camOrientation = Quaternion.Lerp(current, forward, t);
			shouldUseCamOrientation = true;
			t += 1.0f * interval;
			yield return new WaitForSeconds(interval);
		}
		shouldUseCamOrientation = false;
		
		//wait half a second
		yield return new WaitForSeconds(0.5f);
		
		//start the countdown
		Train_Rescue game = (Train_Rescue)Component.FindObjectOfType(typeof(Train_Rescue));
		if(game)
		{
			game.StartCountdown();	
		}
		else
		{
			UnityEngine.Debug.LogWarning("CameraFlythrough: couldn't find game");	
		}
		UnityEngine.Debug.Log("Flythrough: completed. starting game");
		
		//reset the gyros here
		MinimalSensorCamera cam = (MinimalSensorCamera)GameObject.FindObjectOfType(typeof(MinimalSensorCamera));
		cam.ResetGyro();
		
//		//flash up another subtitle card warning of the train
//		yield return new WaitForSeconds(0.5f);
//		
//		UnityEngine.Debug.Log("CameraFly: Changing to 'oh no!' subtitle");
//		DataVault.Set("train_subtitle", "\"Oh No! A Train!\"");
	}
	
	protected void FollowFlowLinkNamed(string linkName)
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find( r => r.Name == linkName );
		if(gConnect != null)
		{
			fs.parentMachine.FollowConnection(gConnect);
		}
		else
		{
			UnityEngine.Debug.LogWarning("Train, CameraFlythrough: Couldn't fine flow exit named " + linkName);
		}
	}

	
	//smoothly add a bit of height as we fly through
	float getHeightDelta()
	{
		//use a sin wave
		float heightDeltaScale = Mathf.Sin( Mathf.PI * parametricDist);
		return heightDeltaScale * heightDeltaMagnitude;
	}
	
//	void OnGUI() {
//		if(state != FlythroughState.Finished)
//		{
//			//render some black bars top and bottom of screen
//		}
//		if(showSubtitleCard)
//		{
//			//if we're at the end, show the subtitle card
//			Texture tex = Resources.Load("Train_Rescue/SubtitleCard_Save", typeof(Texture)) as Texture;
//			Rect textureRect = new Rect(0, 0, Screen.width, Screen.height);
//			GUI.DrawTexture(textureRect, tex, ScaleMode.ScaleToFit, true);
//		}
//			
//	}
}
