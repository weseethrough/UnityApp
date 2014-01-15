using UnityEngine;
using System.Collections;
using System;

public class Train_Rescue : GameBase {
	
	protected TrainController_Rescue train;
	public GameObject trainObject;
	
	float junctionSpacing = 200.0f;
	bool bFailed = false;
	
	// Use this for initialization
	void Start () {
		base.Start();
		
		train = trainObject.GetComponent<TrainController_Rescue>();
		
		//set up a series of junctions for the train
		//beginning at 200m
		float junctionDist = 200.0f;
		
		selectedTrack = (Track)DataVault.Get("current_track");
		
		try {
			if(selectedTrack != null) {
				finish = (int)selectedTrack.distance;
			} else {
				finish = (int)DataVault.Get("finish");
			}
		} catch(Exception e) {
			finish = 5000;	
		}
		
		UnityEngine.Debug.Log("Train: finish = " + finish);
		
		bool bDoneFirstOne = false;
		
		while(junctionDist < (finish - 500.0f))
		{
			//create a junction
			GameObject junctionObject = (GameObject)Instantiate(Resources.Load("TrainJunction"));
			TrainTrackJunction junction = junctionObject.GetComponent<TrainTrackJunction>();
			junction.setTrain(trainObject);
			
			if(!bDoneFirstOne)
			{
				junction.SwitchOnDetour();
				bDoneFirstOne = true;
			}
			
			junction.distancePosition = junctionDist;
			
			//move distance along
			junctionDist += junctionSpacing;
			
			UnityEngine.Debug.Log("Train: Created junction at " + junctionDist);
		}
	}
		
	protected override double GetDistBehindForHud ()
	{
		return train.GetDistanceBehindTarget();
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		
		//check if the train has reached the end
		if(train.GetForwardDistance() > finish)
		{
			//finish the game
			FinishGame();
		}
	}
	
	public override void SetReadyToStart (bool ready)
	{
		base.SetReadyToStart (ready);
		train.BeginRace();
	}
	
	public override GConnector GetFinalConnection ()
	{
		if(bFailed)
		{
			//return a connector to the failure screen
			return null;
		}
		else
		{
			//return a connector to the success screen	
			return null;
		}
	}
}
