using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class PursuitGame : GameBase {
	
	// Enums for the actor types
	public enum ActorType
	{
		Boulder			= 1,
		Eagle			= 2,
		Train			= 3,
		Zombie			= 4,
		Dinosaur        = 5,
		Fire			= 6
	}
	
	// Variable for kilometer bonus
	//private int bonusTarget = 1000;
	
	private ActorType currentActorType;
	
	private GestureHelper.OnTap tapHandler;
	
	public GameObject eagleHolder;
	public GameObject boulderHolder;
	public GameObject zombieHolder;
	public GameObject trainHolder;
	public GameObject dinoHolder;
	public GameObject fireHolder;
	
	private bool finished = false;
	
	//private double offset = 0;
	
	private int lives = 1;
	

	private bool authenticated = false;
	
	private bool isDead = false;
	
	// Use this for initialization
	public override void Start () {
		base.Start();
		
		UnityEngine.Debug.Log("PursuitGame: started");
		
		string tar = (string)DataVault.Get("type");		
		
		switch(tar)
		{
		case "Boulder":
			currentActorType = ActorType.Boulder;
			break;
			
		case "Eagle":
			currentActorType = ActorType.Eagle;
			break;
			
		case "Zombie":
			currentActorType = ActorType.Zombie;
			break;
			
		case "Train":
			currentActorType = ActorType.Train;
			break;
			
		case "Dinosaur":
			currentActorType = ActorType.Dinosaur;
			break;
			
		case "Fire":
			currentActorType = ActorType.Fire;
			break;
			
		default:
			UnityEngine.Debug.Log("PursuitGame: ERROR! No type specified");
			currentActorType = ActorType.Train;
			break;			
		}

		//if we've gone straight to game, pick a 5km run
#if !UNITY_EDITOR
		finish = (int)DataVault.Get("finish");		
#else
		finish = 5000;
#endif
		
		DataVault.Set("slider_val", 0.8f);
		
		// Set templates' active status
		eagleHolder.SetActive(false);
		boulderHolder.SetActive(false);
		zombieHolder.SetActive(false);
		trainHolder.SetActive(false);
		dinoHolder.SetActive(false);
		fireHolder.SetActive(false);
		
		Platform.Instance.ResetTargets();
		Platform.Instance.CreateTargetTracker(targSpeed);
		
		InstantiateActors();
	}
	

//	public override void OnGUI() {
//		base.OnGUI();
//		
//		//update the lives readout
//		if(isDead) 
//		{
//			GUI.Label(new Rect(300, 0, 200, 200), "Lives left: " + lives.ToString(), getLabelStyle() );
//		}
//		
//
//	}
	
	void Update () {
		base.Update();		
				
		//check if we're dead, and remove a life and show 'game over' menu if so.
		if(Platform.Instance.GetLowestDistBehind() - offset >= 0)
		{
			Platform.Instance.StopTrack();
			
			if(lives > 0) {
				lives -= 1;
				isDead = true;
				offset += 50;
				foreach (GameObject actor in actors) {
					actor.GetComponent<TargetController>().IncreaseOffset();
				}
				started = false;
				countdown = false;
				countTime = 3.0f;
			} else if(!finished) {
				//restart - should consolidate this with the 'back' function
				DataVault.Set("total", Platform.Instance.GetCurrentPoints() + Platform.Instance.GetOpeningPointsBalance());
				DataVault.Set("ahead_col_header", "D20000FF");
				DataVault.Set("ahead_col_box", "D20000EE");
				DataVault.Set("finish_header", "You died!");
				FlowState fs = FlowStateMachine.GetCurrentFlowState();
				GConnector gConect = fs.Outputs.Find(r => r.Name == "FinishButton");
				if(gConect != null) {
					finished = true;
					tapHandler = new GestureHelper.OnTap(() => {
						UnityEngine.Debug.Log("FinishListener: Setting tap handler");
						ContinueGame();
					});
					GestureHelper.onTap += tapHandler;
					fs.parentMachine.FollowConnection(gConect);
				} else {
					UnityEngine.Debug.Log("Game: No connection found!");
				}
			}
		}
		
		// Awards the player points for running certain milestones
		if(Platform.Instance.Distance() >= bonusTarget)
		{
			int targetToKm = bonusTarget / 1000;
			if(bonusTarget < finish) 
			{
				MessageWidget.AddMessage("Bonus Points!", "You reached " + targetToKm.ToString() + "km! 1000pts", "trophy copy");
			}
			bonusTarget += 1000;
			
		}
		
		if(Platform.Instance.Distance() >= finish - 100)
		{
			DataVault.Set("ending_bonus", "Keep going for " + finalBonus.ToString("f0") + " bonus points!");
			finalBonus -= 50f * Time.deltaTime;
		}
		else
		{
			DataVault.Set("ending_bonus", "");
		}
		

	}
	
	void ContinueGame() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "ContinueButton");
		UnityEngine.Debug.Log("FinishListener: Finding output");
		if(gConnect != null)
		{
			(gConnect.Parent as Panel).CallStaticFunction(gConnect.EventFunction, null);
			fs.parentMachine.FollowConnection(gConnect);
			started = false;
		}
		else
		{
			UnityEngine.Debug.Log("FinishListener: Couldn't find connection - continue button");
		}
	}

	
	// Instantiate target actors based on actor type
	void InstantiateActors() {				
		UnityEngine.Debug.Log("PursuitGame: instantiating actors");
		// Remove current actors
		foreach (GameObject actor in actors) {
			Destroy(actor);
		}
		actors.Clear();
		
		GameObject template;
		switch(currentActorType) {
		case ActorType.Eagle:
			template = eagleHolder;
			break;
		case ActorType.Boulder:
			template = boulderHolder;
			break;
		case ActorType.Train:
			template = trainHolder;
			break;
		case ActorType.Zombie:
			template = zombieHolder;
			break;
		case ActorType.Dinosaur:
			template = dinoHolder;
			break;
		case ActorType.Fire:
			template = fireHolder;
			break;
		default:
			throw new NotImplementedException("PursuitGame: Unknown actor type: " + currentActorType);
			break;
		}
		
#if !UNITY_EDITOR
		List<TargetTracker> trackers = Platform.Instance.targetTrackers;
		foreach (TargetTracker tracker in trackers) {
			GameObject actor = Instantiate(template) as GameObject;
			TargetController controller = actor.GetComponent<TargetController>();
			if (controller == null) Debug.Log("PursuitGame: ERROR! Null controller for " + actor.ToString());
			controller.SetTracker(tracker);
			controller.IncreaseOffset(); // TODO: Change. This is not clean
			actor.SetActive(true);
			actors.Add(actor);
		}
#else
		GameObject actorDummy = Instantiate(template) as GameObject;
		actorDummy.SetActive(true);
		actors.Add(actorDummy);
#endif
		offset = 50;
	}
		

	
	public void OnSliderValueChange() 
	{
		gameParamsChanged = true;
		targSpeed = UISlider.current.value * 10.4f;
		UnityEngine.Debug.Log(UISlider.current.value);
	}
	
	public void SetActorType(ActorType type) {
		currentActorType = type;
		gameParamsChanged = true;
	}
	
	public void OnDestroy() {
		GestureHelper.onTap -= tapHandler;
	}
}
