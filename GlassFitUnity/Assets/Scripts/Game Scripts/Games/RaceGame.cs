using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;


public class RaceGame : GameBase {
	 
	// Enums for the actor types
	public enum ActorType
	{
		Runner			= 1,
		Cyclist			= 2,
		Mo				= 3,
		Paula 			= 4
	}
	
	private ActorType currentActorType = ActorType.Runner;

	
//	// Minimap attributes
//	private GameObject minimap;
//	private const int MAP_RADIUS = 1;
//	Texture2D selfIcon;
//	Texture2D targetIcon;
//	Texture2D mapTexture = null;
//	Material mapStencil;
//	const int mapAtlasRadius = 315; // API max width/height is 640
//	const int mapZoom = 18;
//	Position mapOrigo = new Position(0, 0);
//	WWW mapWWW = null;
//	Position fetchOrigo = new Position(0, 0);
	
//	private Rect debug;
	//private const int MARGIN = 15;
	
	// Holds actor templates
	public GameObject cyclistHolder;
	public GameObject runnerHolder;
	public GameObject moHolder;
	public GameObject paulaHolder;
	
	

	
	// Target for bonus points
	private int bonusTarget = 1000;



	
	void Start () {
		base.Start();
		
		//instantiate teh appropriate actor
		string tar = (string)DataVault.Get("type");
		
		switch(tar)
		{
		case "Runner":
			currentActorType = ActorType.Runner;
			break;
			
		case "Cyclist":
			currentActorType = ActorType.Cyclist;
			break;
			
		case "Mo":
			currentActorType = ActorType.Mo;
			DataVault.Set("slider_val", 0.525f);
			targSpeed = 6.059f;
			break;
			
		case "Paula":
			currentActorType = ActorType.Paula;
			DataVault.Set("slider_val", 0.4f);
			targSpeed = 4.91f;
			break;
		}
		
		// Set templates' active status
		cyclistHolder.SetActive(false);
		runnerHolder.SetActive(false);
		
		
		//

		// TODO: Move tracker creation to a button/flow and keep this class generic
		//if (Platform.Instance.targetTrackers.Count == 0) {
		Platform.Instance.ResetTargets();
		Platform.Instance.CreateTargetTracker(targSpeed);
		//} // else trackers created earlier

		
		InstantiateActors();

	}
	
	public void SetActorType(ActorType targ) {
		currentActorType = targ;
		gameParamsChanged = true;		
	}
	
	public void OnGUI()
	{

	}
		

	
	
	protected void UpdateLeaderboard() {
		double distance = Platform.Instance.Distance();
		// TODO: Decide if we are allowed to sort in place or need to make a copy
		List<TargetTracker> trackers = Platform.Instance.targetTrackers;
		trackers.Sort(delegate(TargetTracker x, TargetTracker y) {
			return y.GetTargetDistance().CompareTo(x.GetTargetDistance());
		} );
		int position = 1;
		foreach (TargetTracker tracker in trackers) {
			if (tracker.GetTargetDistance() > distance) position++;
		}

		DataVault.Set("ahead_col_box", "D20000EE");
		
		DataVault.Set("leader_header", "You are");
		if (position == 1) { 
			DataVault.Set("ahead_leader", "in the lead!");
			DataVault.Set("ahead_col_box", "19D200EE");
		}  else {
			DataVault.Set("ahead_leader", "behind by " + SiDistance(trackers[0].GetDistanceBehindTarget()));
		}
		
		DataVault.Set("position_header", "Position");
		string nth = position.ToString();
		if (position == 1) nth += "st";
		if (position == 2) nth += "nd";
		if (position == 3) nth += "rd";
		if (position >= 4) nth += "th";
		if (position > 2 && position == trackers.Count + 1) nth = "Last!";
		DataVault.Set("position_box", nth);
		
		// Find closest (abs) target
		TargetTracker nemesis = null;
		TargetTracker upstream = null;
		if (position > 1) upstream = trackers[position - 2]; // 1->0 indexing
		TargetTracker downstream = null;
		if (position < trackers.Count + 1) downstream = trackers[position - 1]; // 1->0 indexing
			
		if (upstream != null && downstream != null) {
			if (Math.Abs(upstream.GetDistanceBehindTarget()) <= Math.Abs(downstream.GetDistanceBehindTarget())) nemesis = upstream;
			else nemesis = downstream;
		}  
		else if (upstream != null) nemesis = upstream;
		else if (downstream != null) nemesis = downstream;		
		
		if (nemesis != null) {
			double d = nemesis.GetDistanceBehindTarget();
			string which = " behind";
			if (d > 0) which = " ahead";
			DataVault.Set("follow_header", nemesis.name + " is"); 
			DataVault.Set("follow_box", SiDistance(Math.Abs(d)) + which);
		}  else {
			DataVault.Set("follow_header", "Solo");
			DataVault.Set("follow_box", "round!");
		}
	}
	

	
	void Update () {
		
		// Awards the player points for running certain milestones
		if(Platform.Instance.Distance() >= bonusTarget)
		{
			int targetToKm = bonusTarget / 1000;
			if(bonusTarget < targetDistance) 
			{
				MessageWidget.AddMessage("Bonus Points!", "You reached " + targetToKm.ToString() + "km! 1000pts", "trophy copy");
			}
			bonusTarget += 1000;
			
		}
		
		// Gives the player bonus points for sprinting the last 100m
		if(Platform.Instance.Distance() >=  - 100)
		{
			DataVault.Set("ending_bonus", "Keep going for " + finalBonus.ToString("f0") + " bonus points!");
			finalBonus -= 50f * Time.deltaTime;
		}
		else
		{
			DataVault.Set("ending_bonus", "");
		}
	
		UpdateLeaderboard();
	}
	
	
	// Instantiate target actors based on actor type
	void InstantiateActors() 
	{				
		// Remove current actors
		foreach (GameObject actor in actors) {
			Destroy(actor);
		}
		actors.Clear();
		
		GameObject template;
		switch(currentActorType) {
		case ActorType.Runner:
			template = runnerHolder;
			break;
		case ActorType.Cyclist:
			template = cyclistHolder;
			break;
		case ActorType.Mo:
			template = moHolder;
			targSpeed = 6.059f;
			DataVault.Set("slider_val", 0.525f);
			break;
		default:
			throw new NotImplementedException("Unknown actor type: " + currentActorType);
			break;
		}
		
		
//Don't create target trackers in the editor, since these rely on the real Platform being around, for now.
#if !UNITY_EDITOR
		List<TargetTracker> trackers = Platform.Instance.targetTrackers;
		int lane = 1;
		foreach (TargetTracker tracker in trackers) {
			GameObject actor = Instantiate(template) as GameObject;
			TargetController controller = actor.GetComponent<TargetController>();
			controller.SetTracker(tracker);
			controller.SetLane(lane++);
			actor.SetActive(true);
			actors.Add(actor);
		}
#else
		GameObject actorDummy = Instantiate(template) as GameObject;
		actorDummy.SetActive(true);
		actors.Add(actorDummy);
		UnityEngine.Debug.Log("RaceGame: instantiated actors");
#endif
	}
	
	public void OnSliderValueChange() 
	{
		gameParamsChanged = true;
		targSpeed = UISlider.current.value * 10.4f;
		UnityEngine.Debug.Log(UISlider.current.value);
	}
	
	// Listen for UnitySendMessage with multiplier updates
	// Display the ner multiplier on screen for a second or so

	
}

