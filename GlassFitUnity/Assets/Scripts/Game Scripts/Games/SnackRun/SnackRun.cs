using UnityEngine;
using System.Collections;

public class SnackRun : GameBase {
	
	protected SnackController snackController = null;
	
	//snack every 250m
	protected float nextSnackDistance = 50.0f;
	protected float snackInterval = 200.0f;
	
	// Use this for initialization
	void Start() {
		base.Start();
		//create snack controller
		snackController = new GameObject().AddComponent<SnackController>();
	}
	
	// Update is called once per frame
	void Update () {
		float playerDistance = Platform.Instance.GetDistance();
		if( playerDistance > nextSnackDistance )
		{
			if(snackController != null)
			{
				snackController.OfferGame();
			}
			nextSnackDistance += snackInterval;
		}
		
		base.Update();
	}
	
	public void OnSnackFinished()
	{
		//queue up the next snack offer.
	}
	
	
}
