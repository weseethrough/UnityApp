using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using RaceYourself;

public class RaceGame : GameBase {
	 
	public bool end = false;

    /// <summary>
    /// In-scene GameObject where instantiated actors should be placed.
    /// </summary>
    public GameObject actors;
    /// <summary>
    /// Generic actor prefab. Additional info about the actor is added to the actor post-load.
    /// </summary>
    public GameObject actor;
	
	private ActorActivity currentActorActivity = ActorActivity.Runner;

    public List<WorldObject> worldObjects {get; private set;}

    protected GameObject opponent; // TODO loop over actors instead, excluding player
    protected GameObject player;

	private float targSpeed = 2.4f;
    
    public RaceGame()
    {
        worldObjects = new List<WorldObject>();
    }

	public override void Start ()
    {
		base.Start();
        
        //instantiate the appropriate actor
        string target = (string)DataVault.Get("type");
        if(target == null)
        {
            target = "Runner";
        }
        
        switch(target)
        {
        case "Runner":
            currentActorActivity = ActorActivity.Runner;
            ////opponent = runnerHolder;
            targSpeed = 3.0f;
            break;
            
        case "Cyclist":
            currentActorActivity = ActorActivity.Cyclist;
            ////opponent = cyclistHolder;
            targSpeed = 2.4f;
            break;
        }
        
        Platform.Instance.ResetTargets();

        View view = gameObject.GetComponent<View>();

        player = (GameObject) Instantiate(actor);
        player.transform.parent = actors.transform; // place in scene.
        player.AddComponent<PlayerPositionController>(); // control movement using user movement
        view.AddVisualRepresentation(player, currentActorActivity, Platform.Instance.api.user);
        
        opponent = (GameObject) Instantiate(actor);
        opponent.transform.parent = actors.transform;
        view.AddVisualRepresentation(opponent, currentActorActivity, null); // TODO pass in user
		//opponent.SetActive(true);

		if(selectedTrack != null)
        {
			TrackPositionController posController = opponent.AddComponent<TrackPositionController>();
			posController.setTrack(selectedTrack);
		}
		else
        { // Fallback: give opponent a fixed pace.
			ConstantVelocityPositionController posController = opponent.AddComponent<ConstantVelocityPositionController>();
			posController.velocity = new Vector3(0, 0, targSpeed);
		}

		//Platform.Instance.LocalPlayerPosition.SetIndoor(true);
		//SetReadyToStart(true);
	}
	
	public void SetActorType(ActorActivity targ)
    {
		currentActorActivity = targ;
	}

    // TODO should reference world position of player (...which should in turn be their distance)
	protected override double GetDistBehindForUI ()
	{
		WorldObject opponentWorldObj = opponent.GetComponent<WorldObject>();
		return opponentWorldObj.getRealWorldPos().z - (float) Platform.Instance.LocalPlayerPosition.Distance;
	}

	protected void UpdateLeaderboard()
    {
		double distance = Platform.Instance.LocalPlayerPosition.Distance;
		// TODO: Decide if we are allowed to sort in place or need to make a copy
		List<TargetTracker> trackers = Platform.Instance.targetTrackers;
		int position = 1;
		
		if(trackers != null)
        {
			trackers.Sort(delegate(TargetTracker x, TargetTracker y) {
				return y.GetTargetDistance().CompareTo(x.GetTargetDistance());
			});
		
			foreach (TargetTracker tracker in trackers)
            {
				if (tracker.GetTargetDistance() > distance) position++;
		    }
		}
		DataVault.Set("ahead_col_box", UIColour.red);
		
		DataVault.Set("leader_header", "You are");
		if (position == 1)
        { 
			DataVault.Set("ahead_leader", "in the lead!");
			DataVault.Set("ahead_col_box", UIColour.green);
		}
        else
        {
			DataVault.Set("ahead_leader", "behind by " + UnitsHelper.SiDistance(trackers[0].GetDistanceBehindTarget()));
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
		if (position > 1)
            upstream = trackers[position - 2]; // 1->0 indexing
		TargetTracker downstream = null;
		if (position < trackers.Count + 1)
			downstream = trackers[position - 1]; // 1->0 indexing
			
		if (upstream != null && downstream != null)
        {
			if (Math.Abs(upstream.GetDistanceBehindTarget()) <= Math.Abs(downstream.GetDistanceBehindTarget()))
                nemesis = upstream;
			else
                nemesis = downstream;
		}  
		else if (upstream != null)
            nemesis = upstream;
		else if (downstream != null)
            nemesis = downstream;		
		
		if (nemesis != null)
        {
			double d = nemesis.GetDistanceBehindTarget();
			string which = " behind";
			if (d > 0) which = " ahead";
			DataVault.Set("follow_header", nemesis.name + " is"); 
			DataVault.Set("follow_box", UnitsHelper.SiDistance(Math.Abs(d)) + which);
		}
        else
        {
			DataVault.Set("follow_header", "Solo");
			DataVault.Set("follow_box", "round!");
		}
	}
}
