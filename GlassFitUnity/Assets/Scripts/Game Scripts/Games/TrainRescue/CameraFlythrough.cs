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
	protected float heightDeltaMagnitude = 50.0f;
	
	protected float xOffset = 0.0f;
	protected float height = 0.0f;
	
	bool showSubtitleCard = false;
	
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
		
		//make it so we stop short of the damsel herself.
		finish -= endOffsetDist;
		
		xOffset = transform.localPosition.x;
		height = transform.localPosition.y;
		
	}
	
	public void StartFlythrough()
	{
		state = FlythroughState.Forward;
	}
	
	// Update is called once per frame
	void Update () {
		float distance = 0.0f;
		GameObject lookAtTarget = null;
		GameObject damsel = GameObject.Find("Tracks_Damsel");

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
				StartCoroutine(MidFlythroughSequence());
				distance = parametricDist * finish;
				lookAtTarget = damsel;
				break;
			}
			case FlythroughState.Backward:
			{
				parametricDist -= Time.deltaTime * parametricFlySpeed;
				if(parametricDist < 0.0f) 
				{ 
					parametricDist = 0.0f; 
					state = FlythroughState.ViewTrain;
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
				}
				float offset = 50.0f;	//we'll be this far back from the train
				distance = parametricDist * (finish + offset) - offset;
				break;
			}
		case FlythroughState.ViewTrain:
		{
			//turn towards train	
			break;
		}
		}
		
		//add lofted height delta
		//set transform. Player distance should be zero at this point.
		transform.localPosition = new Vector3(xOffset, height + getHeightDelta(), distance);
		
		//look at the damsel
		if(lookAtTarget)
		{
			Vector3 lookAtPos = damsel.transform.localPosition;
			//fudge downwards. Seems necessary for some reason;
			transform.LookAt(lookAtPos);
		}
		else
		{
			//look dead ahead
			
		}
	}
	
	IEnumerator MidFlythroughSequence()
	{
		//wait a few seconds
		yield return new WaitForSeconds(1.0f);
		
		//show the subtitle card for a second
		FollowFlowLinkNamed("Subtitle");		
		
		DataVault.Set("train_subtitle", "\"Help\"\n\"Please Save Me!\"");
		yield return new WaitForSeconds(2.0f);
		
		//back to HUD
		FollowFlowLinkNamed("Resume");
		
		//wait another second on the maiden
		yield return new WaitForSeconds(1.0f);
		
		//move to next state of flythrough
		state = FlythroughState.Backward;
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
