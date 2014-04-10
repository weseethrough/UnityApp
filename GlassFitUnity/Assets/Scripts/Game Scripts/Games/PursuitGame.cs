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
    
	private TargetController currentOpponent = null;
	
	private new GestureHelper.OnTap tapHandler;
	
	public TargetController eagleHolder;
	public TargetController boulderHolder;
	public TargetController zombieHolder;
	public TargetController trainHolder;
	public TargetController dinoHolder;
	public TargetController fireHolder;
	
	private bool finished = false;
	
	private int lives = 1;

	private bool authenticated = false;
	
	private bool isDead = false;
	
	// Use this for initialization
	public override void Start () {
		base.Start();
		
		UnityEngine.Debug.Log("PursuitGame: started");
		
		// Set templates' active status
		eagleHolder.gameObject.SetActive(false);
		boulderHolder.gameObject.SetActive(false);
		zombieHolder.gameObject.SetActive(false);
		trainHolder.gameObject.SetActive(false);
		dinoHolder.gameObject.SetActive(false);
		fireHolder.gameObject.SetActive(false);
		
		string tar = (string)DataVault.Get("type");		

		switch(tar)
		{
		case "Boulder":
			currentActorType = ActorType.Boulder;
			currentOpponent = boulderHolder;
			break;
		case "Eagle":
			currentActorType = ActorType.Eagle;
			currentOpponent = eagleHolder;
			break;
			
		case "Zombie":
			currentActorType = ActorType.Zombie;
			currentOpponent = zombieHolder;
			break;
		case "Train":
			currentActorType = ActorType.Train;
			currentOpponent = trainHolder;
			break;
		case "Dinosaur":
			currentActorType = ActorType.Dinosaur;
			currentOpponent = dinoHolder;
			break;
		case "Fire":
			currentActorType = ActorType.Fire;
			currentOpponent = fireHolder;
			break;
		default:
			UnityEngine.Debug.Log("PursuitGame: ERROR! No type specified");
			currentActorType = ActorType.Train;
			currentOpponent = trainHolder;
			break;
		}

		currentOpponent.gameObject.SetActive(true);

		//if we've gone straight to game, pick a 5km run
#if !UNITY_EDITOR
		finish = (int)DataVault.Get("finish");		
#else
		finish = 5000;
#endif

	}
	
	public override void Update () {
		base.Update();		
				
		//check if we're dead, and remove a life and show 'game over' menu if so.
		if(Platform.Instance.GetLowestDistBehind() >= 0)
		{
			Platform.Instance.LocalPlayerPosition.StopTrack();
			
			if(lives > 0) {
				lives -= 1;
				isDead = true;
				//move the target actor back to original offset distance
				currentOpponent.setRealWorldDist((float)Platform.Instance.LocalPlayerPosition.Distance - 50);

				started = false;
			} else if(!finished) {

				DataVault.Set("total", Platform.Instance.PlayerPoints.CurrentActivityPoints + Platform.Instance.PlayerPoints.OpeningPointsBalance);
				DataVault.Set("ahead_col_header", UIColour.red);
				DataVault.Set("ahead_col_box", UIColour.red);
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
		if(Platform.Instance.LocalPlayerPosition.Distance >= bonusTarget)
		{
			int targetToKm = bonusTarget / 1000;
			if(bonusTarget < finish) 
			{
				MessageWidget.AddMessage("Bonus Points!", "You reached " + targetToKm.ToString() + "km! 1000pts", "trophy copy");
			}
			bonusTarget += 1000;
			
		}
		
		if(Platform.Instance.LocalPlayerPosition.Distance >= finish - 100)
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
		
	public void OnDestroy() {
		GestureHelper.onTap -= tapHandler;
	}
}
