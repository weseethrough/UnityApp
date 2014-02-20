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
	
	// Use this for initialization
	void Start() {
		base.Start();
		//create snack controller
		snackController = new GameObject().AddComponent<SnackController>();
		
		ClearAheadBehind();
		
		chimeSound = GetComponent<AudioSource>() as AudioSource;
		
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
		float playerDistance = Platform.Instance.GetDistance();
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
			snackController.OfferGame();
			snackActive = true;
			TriggerAlert();
			minigameToken.gameObject.SetActive(false);
		}
		else
		{
			UnityEngine.Debug.LogError("SnackRun: Couldn't find snack controller");
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
	
	public void OnSnackFinished()
	{
		//queue up the next snack offer.
		float currentDistance = Platform.Instance.GetDistance();
		nextSnackDistance = currentDistance + snackInterval;
		
		//shift token along and unhide
		if(minigameToken != null)
		{ 
			minigameToken.SetDistance(nextSnackDistance);
			minigameToken.gameObject.SetActive(true);
		}
		
		UnityEngine.Debug.Log("SnackRun: Snack finished. Next snack at " + nextSnackDistance);
		snackActive = false;
		ClearAheadBehind();
	}
	
	
}
