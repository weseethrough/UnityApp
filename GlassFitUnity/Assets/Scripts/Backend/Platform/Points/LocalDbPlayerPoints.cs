using System;
using System.Threading;
using Sqo;
using SiaqodbDemo;
using RaceYourself.Models;
using UnityEngine;


public class LocalDbPlayerPoints : PlayerPoints
{
	
	private const int BASE_POINTS_PER_METRE = 5;
    private const int BASE_MULTIPLIER_LEVELS = 4;
    private const int BASE_MULTIPLIER_PERCENT = 25;
    private const long BASE_MULTIPLIER_TIME_THRESH = 8;  // seconds
	
	private Siaqodb db = DatabaseFactory.GetInstance();
	private Log log = new Log("PlayerPoints");
	
	private long _currentActivityPoints = 0;
	private long _lastTransactionPoints;
	public override long CurrentActivityPoints { get { return _currentActivityPoints; } }
	
	private long _openingPointsBalance = 0;
	public override long OpeningPointsBalance { get { return _openingPointsBalance; } }
	
	private int _currentGemBalance = 0;
	public override int CurrentGemBalance { get { return _currentGemBalance; } }
	
	private float _currentMetabolism = 100.0f;
	public override float CurrentMetabolism	{ get { return 100 + _currentMetabolism; } }
	
	private float basePointsSpeed = 0.0f;
	
	private Transaction lastTransaction;
	
	private long lastCumulativeTime = 0L;
    private double lastCumulativeDistance = 0.0;
    private int lastBaseMultiplierPercent = 100;
	private long lastMultiplierCheckTime = 0L;
	private double lastMultiplierCheckDistance = 0.0;
	
	public LocalDbPlayerPoints()
	{
		// TODO: ensure last returns transaction with greatest  timestamp
		lastTransaction = db.Cast<Transaction>().LastOrDefault<Transaction>();
		if (lastTransaction == null) {
			SaveToDatabase("INITIALISE","Everything set to zero", "PlayerPoints");  // first launch of game
			log.info("Set initial points and gems to zero, and metabolism to 100");
			return;
		}
		
		_openingPointsBalance = lastTransaction.points_balance;
		_lastTransactionPoints = lastTransaction.points_balance;
		_currentGemBalance = lastTransaction.gems_balance;
		_currentMetabolism = lastTransaction.metabolism_balance;
		
		// Decay metabolism for inactiviy since last workout
		long decayTime = (UnitsHelper.MillisSince1970(DateTime.Now) - lastTransaction.ts);
		_currentMetabolism *= Mathf.Exp(-(float)decayTime*0.00000001f);
		TimeSpan decayTimeSpan = new TimeSpan(decayTime*100000);
		String calc = String.Format("Metabolism decayed from " + lastTransaction.metabolism_balance + " to " +
            _currentMetabolism + " since last workout. Time of inactivity is {0:N0} days, {1} hours, {2} minutes, {3} seconds", 
                decayTimeSpan.Days, decayTimeSpan.Hours, decayTimeSpan.Minutes, decayTimeSpan.Seconds);
		SaveToDatabase("BETWEEN-GAME METABOLISM DECAY", calc, "PlayerPoints");
		log.info(calc);
	}

	public override void Update()
	{
		if (!Platform.Instance.LocalPlayerPosition.IsTracking) return;  //TODO: should decay metabolism here
        
		double currentDistance = Platform.Instance.LocalPlayerPosition.Distance;
		long currentTime = Platform.Instance.LocalPlayerPosition.Time;
		
        if (currentDistance + 5 < lastCumulativeDistance) {
            // user has probably reset/restarted the route, need to re-init.
            // TODO: work out how to award/save the points earned between last task.run and now. (currently they are discarded)
            lastCumulativeDistance = currentDistance;
            lastCumulativeTime = currentTime;
            lastBaseMultiplierPercent = 100;
			return;
        }
		
		// increment base points
		double awardDistance = currentDistance - lastCumulativeDistance;
        long pointsDelta = (long)(awardDistance * lastBaseMultiplierPercent * BASE_POINTS_PER_METRE) / 100; //integer division floors to nearest whole point below
		_currentActivityPoints += pointsDelta;
		
		// increment metabolism
        float metabolismDelta = (float)Mathf.Exp(-(CurrentMetabolism-100)/20) // the more you have, the harder it is to earn
                /(60*BASE_MULTIPLIER_TIME_THRESH); // scale per-minute reward to trigger time of this loop
		_currentMetabolism += metabolismDelta;
            
        // check if we need to update the mutiplier
		if (basePointsSpeed != 0.0f && currentTime > lastMultiplierCheckTime + BASE_MULTIPLIER_TIME_THRESH)
		{
			long multiplierTime = currentTime - lastMultiplierCheckTime; //seconds
			double multiplierDistance = currentDistance - lastMultiplierCheckDistance; //metres
            float multiplierSpeed = (float)(multiplierDistance/multiplierTime);
            if (multiplierSpeed > basePointsSpeed)
			{
                // bump up the multiplier (incremented by BASE_MULTIPLIER_PERCENT each time round this loop for BASE_MULTIPLIER_LEVELS)
                if (lastBaseMultiplierPercent <= (1+BASE_MULTIPLIER_LEVELS*BASE_MULTIPLIER_PERCENT))
				{
                    lastBaseMultiplierPercent += BASE_MULTIPLIER_PERCENT;
                    Platform.Instance.SendMessage("NewBaseMultiplier", lastBaseMultiplierPercent/100.0f);
                    log.info("New base multiplier: " + lastBaseMultiplierPercent + "%");
                }
            } 
			else if (lastBaseMultiplierPercent != 100) 
			{
                // drop multiplier to 1
                lastBaseMultiplierPercent = 100;
                Platform.Instance.SendMessage("NewBaseMultiplier", lastBaseMultiplierPercent/100.0f);
                log.info("New base multiplier: " + lastBaseMultiplierPercent + "%");
            }
			lastMultiplierCheckTime = currentTime;
		}
		

        // save the points & metabolism to the database every BASE_MULTIPLIER_TIME_THRESH
		if (lastMultiplierCheckTime == currentTime && _currentActivityPoints > 0)
		{
			SaveToDatabase("BASE_POINTS","Period = 8s, Multiplier = " + lastBaseMultiplierPercent + "%", "PlayerPoints");
		}
		
		// update the reference points for the next loop
		lastCumulativeDistance = currentDistance;
        lastCumulativeTime = currentTime;
	}

	public override void Reset() {
		SaveToDatabase("GAME RESET", "Outstanding earnings", "PlayerPoints");
		_currentActivityPoints = 0;
		_openingPointsBalance = (lastTransaction == null ? 0 : lastTransaction.points_balance);
		lastCumulativeDistance = 0.0;
        lastCumulativeTime = 0L;
        lastBaseMultiplierPercent = 100;
		lastMultiplierCheckTime = 0L;
		lastMultiplierCheckDistance = 0.0;
	}
	
	/// <summary>
	/// If the user travels at > the base points speed, multipliers will be applied to increase the rate at which points are awarded
	/// Multipliers are broadcast as they are awarded with SendMessage for e.g. display on HUD
	/// </summary>
	/// <param name='speed'>
	/// Speed in m/s.
	/// </param>
	public override void SetBasePointsSpeed (float speed)
	{
		this.basePointsSpeed = speed;
	}
	

     /// <summary>
     /// Flexible helper method for awarding arbitrary in-game points for e.g. custom achievements
     /// </summary>
     /// <param name='reason'>
     /// tring describing how the points were calculated (for human sense check).
     /// </param>
     /// <param name='gameId'>
     /// which bit of code generated the points?.
     /// </param>
     /// <param name='points'>
     /// the points to add/deduct from the user's balance.
     /// </param>
	public override void AwardPoints (string reason, string gameId, long points)
	{
		_currentActivityPoints += points;
		SaveToDatabase("REWARD POINTS", reason, gameId);
        log.info("Awarded " + "REWARD POINTS" + " of "+ points + " points for " + reason + " in " + gameId);
	}

	 /// <summary>
     /// Flexible helper method for awarding arbitrary in-game gems for e.g. custom achievements
     /// </summary>
     /// <param name='reason'>
     /// tring describing how the points were calculated (for human sense check).
     /// </param>
     /// <param name='gameId'>
     /// which bit of code generated the points?.
     /// </param>
     /// <param name='gems'>
     /// the gems to add/deduct from the user's balance.
     /// </param>
	public override void AwardGems (string reason, string gameId, int gems)
	{
		_currentGemBalance += gems;
		SaveToDatabase("REWARD GEMS", reason, gameId);
        log.info ("Awarded " + "REWARD GEMS" + " of "+ gems + " gems for " + reason + " in " + gameId);
	}
	
	
	private void SaveToDatabase(String transactionType, String transactionCalc, String sourceId)
	{
		Transaction t = new Transaction();
		t.points_delta = _currentActivityPoints + _openingPointsBalance - (lastTransaction == null ? 0 : lastTransaction.points_balance);
		t.points_balance = _currentActivityPoints + _openingPointsBalance;
		t.gems_delta = _currentGemBalance - (lastTransaction == null ? 0 : lastTransaction.gems_balance);
		t.gems_balance = _currentGemBalance;
		t.metabolism_delta = _currentMetabolism - (lastTransaction == null ? 0 : lastTransaction.metabolism_balance);
		t.metabolism_balance = _currentMetabolism;
		t.transaction_type = transactionType;
		t.transaction_calc = transactionCalc;
		t.source_id = sourceId;
		t.save(db);
		lastTransaction = t;
	}

}

