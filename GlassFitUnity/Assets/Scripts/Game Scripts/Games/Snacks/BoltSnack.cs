using UnityEngine;
using System.Collections;

public class BoltSnack : SnackBase {
	
	private BoltController bolt;
	
	private bool boltFinished = false;
	
	private bool playerFinished = false;
	
	private bool displayingBanner = false;
	
	private long startTime = 0;
	
	private long finalTime = 0;
	
	// Use this for initialization
	void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		
		if(bolt != null && bolt.enabled)
		{
			UpdateAhead(bolt.GetDistanceBehindTarget());
			if(bolt.GetBoltDistanceTravelled() >= 100 && !boltFinished)
			{
				boltFinished = true;
				if(!playerFinished) {
					DataVault.Set("death_colour", "EA0000FF");
					DataVault.Set("snack_result", "You lost!");
					DataVault.Set("snack_result_desc", "it was inevitable");
					StartCoroutine(ShowBanner());
				}
			}
			
			if(bolt.GetPlayerDistanceTravelled() >= 100 && !playerFinished)
			{
				if(!boltFinished) {
					boltFinished = true;
					DataVault.Set("death_colour", "12D400FF");
					DataVault.Set("snack_result", "You won?!");
					DataVault.Set("snack_result_desc", "you must be cheating!");
					StartCoroutine(ShowBanner());
				}
				playerFinished = true;
				finalTime = Platform.Instance.Time() - startTime;
			}
			
			if(playerFinished && boltFinished)
			{
				StartCoroutine(EndGame());
			}
		}
	}
	
	private IEnumerator EndGame()
	{
		DataVault.Set("countdown_subtitle", "100m time: " + finalTime / 1000 + " seconds");
		yield return new WaitForSeconds(5.0f);
		DataVault.Set("countdown_subtitle", "");
		Finish();
	}
	
	public override void Begin ()
	{
		base.Begin ();
		boltFinished = false;
		playerFinished = false;
		StartCoroutine(DoCountDown());
	}
	
	IEnumerator DoCountDown()
	{
		UnityEngine.Debug.Log("BoltSnack: Starting Countdown Coroutine");
		for(int i=3; i>=0; i--)
		{
			//set value for subtitle. 0 = GO
			string displayString = (i==0) ? "GO !" : i.ToString();
			DataVault.Set("countdown_subtitle", displayString);
			
			//wait half a second
			yield return new WaitForSeconds(1.0f);
		}
		//start the game
		DataVault.Set("countdown_subtitle", " ");
		StartRace();
	}
	
	void StartRace()
	{
		bolt = GetComponent<BoltController>();
		if(bolt)
		{
			bolt.enabled = true;
			startTime = Platform.Instance.Time();
		}
		else
		{
			UnityEngine.Debug.Log("BoltSnack: can't find Bolt controller!");
		}
	}
}
