using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;


public class RaceGame : GameBase {
	 
	public bool end = false;
	
	// Enums for the actor types
	public enum ActorType
	{
		Runner			= 1,
		Cyclist			= 2,
		Mo				= 3,
		Paula 			= 4,
		Chris			= 5,
		Bradley 		= 6
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
	public GameObject chrisHolder;
	public GameObject bradleyHolder;
		
	void Start () {
		base.Start();
		
		//instantiate teh appropriate actor
		string tar = (string)DataVault.Get("type");
		
		switch(tar)
		{
		case "Runner":
			currentActorType = ActorType.Runner;
			targSpeed = 3.0f;
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
			
		case "Chris":
			currentActorType = ActorType.Chris;
			DataVault.Set("slider_val", 0.4f);
			targSpeed = 15.686f;			
			break;
			
		case "Bradley":
			currentActorType = ActorType.Bradley;
			DataVault.Set("slider_val", 0.4f);
			targSpeed = 17.007f;	
			break;
		}
		
		// Set templates' active status
		cyclistHolder.SetActive(false);
		runnerHolder.SetActive(false);
		
		Platform.Instance.ResetTargets();
		
		if(selectedTrack != null) {
			Platform.Instance.CreateTargetTracker(selectedTrack.deviceId, selectedTrack.trackId);
		} else {
			Platform.Instance.CreateTargetTracker(targSpeed);
		}
		
		InstantiateActors();
		
		//Platform.Instance.LocalPlayerPosition.SetIndoor(true);
		//SetReadyToStart(true);
		SetVirtualTrackVisible(true);

	}
	
	public void SetActorType(ActorType targ) {
		currentActorType = targ;
		gameParamsChanged = true;		
	}
	
//	public void OnGUI()
//	{
//		base.OnGUI();
//	}
		

	
	
	protected void UpdateLeaderboard() {
		double distance = Platform.Instance.LocalPlayerPosition.Distance;
		// TODO: Decide if we are allowed to sort in place or need to make a copy
		List<TargetTracker> trackers = Platform.Instance.targetTrackers;
		int position = 1;
		
		if(trackers != null){
			trackers.Sort(delegate(TargetTracker x, TargetTracker y) {
				return y.GetTargetDistance().CompareTo(x.GetTargetDistance());
			} );
		
			foreach (TargetTracker tracker in trackers) {
				if (tracker.GetTargetDistance() > distance) position++;
		}
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

//		
//		// TODO: Multiple minimap targets
//#if !UNITY_EDITOR
//		double targetDistance = Platform.Instance.GetHighestDistBehind();
//		Position position = Platform.Instance.LocalPlayerPosition.Position;
//		float bearing = Platform.Instance.LocalPlayerPosition.Bearing;
//#else
//		double targetDistance = PlatformDummy.Instance.DistanceBehindTarget();
//		Position position = PlatformDummy.Instance.Position();
//		float bearing = PlatformDummy.Instance.Bearing();
//#endif
//		double bearingRad = bearing*Math.PI/180;
////		if (position != null) {
////			// Fake target coord using distance and bearing
////			Position targetCoord = new Position(position.latitude + (float)(Math.Cos(bearingRad)*targetDistance/111229d), position.longitude + (float)(Math.Sin(bearingRad)*targetDistance/111229d));
////			GetMap(position, bearingRad, targetCoord);
////		}

//		if(Platform.Instance.LocalPlayerPosition.Distance >= finish && !end)
//		{
//			end = true;
//			DataVault.Set("total", Platform.Instance.PlayerPoints.CurrentActivityPoints + Platform.Instance.PlayerPoints.OpeningPointsBalance);
//			DataVault.Set("bonus", (int)finalBonus);
//			Platform.Instance.LocalPlayerPosition.StopTrack();
//			GameObject h = GameObject.Find("minimap");
//			if(h != null) {
//				h.renderer.enabled = false;
//			}
//			FlowState fs = FlowStateMachine.GetCurrentFlowState();
//			GConnector gConect = fs.Outputs.Find(r => r.Name == "FinishButton");
//			if(gConect != null) {
//				fs.parentMachine.FollowConnection(gConect);
//			} else {
//				UnityEngine.Debug.Log("Game: No connection found!");
//			}
//		}

	
		base.Update ();
	
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
			targSpeed = 3.0f;
			break;
		case ActorType.Cyclist:
			template = cyclistHolder;
			break;
		case ActorType.Mo:
			template = moHolder;
			targSpeed = 6.059f;
			DataVault.Set("slider_val", 0.525f);
			finish = 10000;
			break;
		case ActorType.Paula:
			template = paulaHolder;
			DataVault.Set("slider_val", 0.4f);
			targSpeed = 4.91f;
			finish = 42195;
			break;
		case ActorType.Chris:
			template = chrisHolder;
			finish = 1000;
			targSpeed = 17.007f;
			break;
		case ActorType.Bradley:
			template = bradleyHolder;
			finish = 4000;
			targSpeed = 15.686f;			
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

