using UnityEngine;
using System.Collections;

public class FirstRaceIndoorOpponent : ConstantVelocityPositionController {
	
	private double playerDistance = 0f;
	private float playerSpeed = 0f;
	
	private float distanceInterval = 50f;
	private float intervalStartTime = 0.0f;
	
	private float distanceFromStart = 0;
	private double lastDistance = 0.0;

	protected FirstRun game = null;

	// Use this for initialization
	public override void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	public override void Update () {
		//UnityEngine.Debug.Log("IndoorOpponent: we are in the update function");
		playerDistance = Platform.Instance.LocalPlayerPosition.Distance;
		
		if(playerDistance > distanceInterval)
		{
			SetRunnerSpeed();
			distanceInterval += 50f;

			//notify main game
			if(game != null)
			{
				game.onLapComplete();
			}
		}

		distanceFromStart += Time.deltaTime * velocity.z;
		
		base.Update();
	}
	
	public void SetRunnerSpeed()
	{
		double currentDistance = Platform.Instance.LocalPlayerPosition.Distance;
		float currentTime = Platform.Instance.LocalPlayerPosition.Time / 1000f;
		
		float intervalTotalTime = currentTime - intervalStartTime;
		
		double newDistance = currentDistance - lastDistance;
		
		float newSpeed = (float)newDistance/intervalTotalTime;
		
		if(newSpeed > velocity.z)
		{
			velocity.z = newSpeed;
		}
		
		lastDistance = currentDistance;
		intervalStartTime = currentTime;
		
		UnityEngine.Debug.Log("IndoorOpponent: speed is " + velocity.z.ToString("f2"));
	}
	
	public void setGame(FirstRun mainGame)
	{
		game = mainGame;
	}
}
