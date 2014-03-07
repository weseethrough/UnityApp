using UnityEngine;
using System.Collections;

public class SnackRun : GameBase {
	
	protected SnackController snackController = null;
	
	public MinigameToken minigameToken = null;
	
	//snack every 250m
	protected float nextSnackDistance = 20.0f;
	protected float snackInterval = 30.0f;
	
	protected float dbg_timer = 0.0f;
	protected bool dbg_hasOfferedSnack = false;
	
	bool snackActive = false;
	
	protected AudioSource chimeSound = null;
	protected AudioSource whooshInSound = null;
	protected AudioSource whooshOutSound = null;
	
	// Use this for initialization
	void Start() {
		
		if( !Platform.Instance.LocalPlayerPosition.IsIndoor() )
		{
			nextSnackDistance = 50.0f;
			snackInterval = 100.0f;
		}
		
		base.Start();
		
		//create snack controller
		snackController = new GameObject().AddComponent<SnackController>();
		
		ClearAheadBehind();
		
		//get audio sources
		Component[] sources = GetComponentsInChildren(typeof(AudioSource));
		chimeSound = (AudioSource)sources[0];
		whooshInSound = (AudioSource)sources[1];
		whooshOutSound = (AudioSource)sources[2];
		
		//initialise minigame 
		if(minigameToken != null)
		{ minigameToken.SetDistance(nextSnackDistance); }
	}

	protected void ClearAheadBehind()
	{
		DataVault.Set("distance_position", "");
		DataVault.Set("target_units", "");
		DataVault.Set("ahead_box", "");	
	}
	
	// Update is called once per frame
	void Update () {
		float playerDistance = (float)Platform.Instance.LocalPlayerPosition.Distance;
		if( !snackActive && playerDistance > nextSnackDistance )
		{
			OfferPlayerSnack();
		}
		
		//debug trigger in editor
#if UNITY_EDITOR
	dbg_timer += Time.deltaTime;
		if(!dbg_hasOfferedSnack && dbg_timer > 5.0f && !snackActive)
		{
			OfferPlayerSnack();
			dbg_hasOfferedSnack = true;
		}
#endif
		
		
		base.Update();
	}
	
	protected void OfferPlayerSnack()
	{
		if(snackController != null)
		{
			UnityEngine.Debug.Log("SnackRun: Offering Snack");
			snackController.OfferGameRotation();
			snackActive = true;
			TriggerAlert();
			minigameToken.gameObject.SetActive(false);
		}
		else
		{
			UnityEngine.Debug.LogError("SnackRun: Couldn't find snack controller");
		}	
	}
	
	public void OfferPlayerSnack(string gameID)
	{
		StartCoroutine(DoSpecificSnackOffer(gameID));
	}
	
	IEnumerator DoSpecificSnackOffer(string gameID)
	{
		//reset the flow to the main HUD, as we could be anywhere in the flow right now.
		FlowStateMachine.Restart("SnackRestartPoint");
		
		//stop countdown
		countdown = false;
		
		yield return new WaitForSeconds(0.5f);
		
		if(snackController != null)
		{
			if(gameID != null)
			{
				snackController.OfferGame(gameID);
				snackActive = true;
				TriggerAlert();
				minigameToken.gameObject.SetActive(false);
			}
			else
			{
				OfferPlayerSnack();	
			}
		}
	}
	
	protected void TriggerAlert()
	{
		 if(chimeSound != null)
		{ chimeSound.Play(); }
	}
	
	public override void GameHandleTap ()
	{
		//see if the snack controller wants to handle the tap first
		if(!snackController.HandleTap())
		{
			//else handle it ourselves
			base.GameHandleTap ();
		}
	}
	
	protected override void UpdateAhead ()
	{
		//do nothing. The snacks handle it in this case.
	}
	
	public void OnSnackBegun()
	{
		//play whoosh in
		//whooshInSound.Play();
	}
	
	/// <summary>
	/// Call when the currently running snack ends, or when the currently offered snack is declined.
	/// </summary>
	public void OnSnackFinished()
	{
		//play whoosh out
		//whooshOutSound.Play();
		
		//queue up the next snack offer.
		float currentDistance = (float)Platform.Instance.LocalPlayerPosition.Distance;
		nextSnackDistance = currentDistance + snackInterval;
		
		//shift token along and unhide
		if(minigameToken != null)
		{ 
			minigameToken.SetDistance(nextSnackDistance);
			minigameToken.gameObject.SetActive(true);
		}
		
		GameObject[] snackObjects = GameObject.FindGameObjectsWithTag("Snacks");
		foreach(GameObject obj in snackObjects)
		{
			Destroy(obj);
		}
		
		UnityEngine.Debug.Log("SnackRun: Snack finished. Next snack at " + nextSnackDistance);
		snackActive = false;
		ClearAheadBehind();
		
		//send bluetooth message
		if(Platform.Instance.IsRemoteDisplay())
		{
			UnityEngine.Debug.Log("SnackRun: Sending Bluetooth message that snack has ended");
			JSONObject json = new JSONObject();
			json.AddField("action", "OnSnackFinished");
			Platform.Instance.BluetoothBroadcast(json);		
		}
	}
	
	protected override void OnFinishedGame ()
	{
		//send Bluetooth message to Remote, if applicable
		if(Platform.Instance.IsRemoteDisplay())
		{
	        JSONObject json = new JSONObject();
			json.AddField("action", "ReturnToMainMenu");
			Platform.Instance.BluetoothBroadcast(json);		
		}
			
		base.OnFinishedGame ();
	}	
	
}
