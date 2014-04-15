using System;
using System.Threading;
using Sqo;
using SiaqodbUtils;
using RaceYourself.Models;
using UnityEngine;


public class LocalDbPlayerPoints : PlayerPoints
{
	
	private const int BASE_POINTS_PER_METRE = 5;
    private const int BASE_MULTIPLIER_LEVELS = 4;
    private const int BASE_MULTIPLIER_PERCENT = 25;
    private const long BASE_MULTIPLIER_TIME_THRESH = 8;  // seconds
	
	private Siaqodb db = DatabaseFactory.GetInstance();
	
	private double _currentActivityPoints = 0;  // floating point to allow incrementing each frame
	private long _lastTransactionPoints;
	public override long CurrentActivityPoints { get { return (long)_currentActivityPoints; } }
	
	private long _openingPointsBalance = 0;
	public override long OpeningPointsBalance { get { return _openingPointsBalance; } }
	
	private int _currentGemBalance = 0;
	public override int CurrentGemBalance { get { return _currentGemBalance; } }
	
	private float _currentMetabolism = 100.0f;
	public override float CurrentMetabolism	{ get { return _currentMetabolism; } }
	
	private float basePointsSpeed = 2.0f;
	
	private Transaction lastTransaction;

	private long lastCumulativeTime = 0L;
    private double lastCumulativeDistance = 0.0;
    private int lastBaseMultiplierPercent = 100;
	private long lastMultiplierCheckTime = 0L;
	private double lastMultiplierCheckDistance = 0.0;
	
	public LocalDbPlayerPoints()
	{
		// TODO: ensure last returns transaction with greatest  timestamp
        log.info("Restoring user's points balance from database");
        try {
            lastTransaction = db.Cast<RaceYourself.Models.Transaction>().Last<RaceYourself.Models.Transaction>();
        } catch (InvalidOperationException e) {
            log.info("Set initial points and gems to zero, and metabolism to 100");
			SaveToDatabase("INITIALISE","Everything set to zero", "PlayerPoints");  // first launch of game
            lastTransaction = db.Cast<RaceYourself.Models.Transaction>().LastOrDefault<RaceYourself.Models.Transaction>();
			return;
		}
		
        log.info("Intialising local fields");
		_openingPointsBalance = lastTransaction.points_balance;
		_lastTransactionPoints = lastTransaction.points_balance;
		_currentGemBalance = lastTransaction.gems_balance;
		_currentMetabolism = lastTransaction.metabolism_balance;
		
		// Decay metabolism for inactiviy since last workout
        log.info("Decaying metabolism");
		long decayTime = (UnitsHelper.MillisSince1970(DateTime.Now) - lastTransaction.ts);
		_currentMetabolism *= Mathf.Exp(-(float)decayTime*0.00000001f);
		TimeSpan decayTimeSpan = new TimeSpan(decayTime*100000);
		String calc = String.Format("Metabolism decayed from " + lastTransaction.metabolism_balance + " to " +
            CurrentMetabolism + " since last workout. Time of inactivity is {0:N0} days, {1} hours, {2} minutes, {3} seconds", 
                decayTimeSpan.Days, decayTimeSpan.Hours, decayTimeSpan.Minutes, decayTimeSpan.Seconds);
        log.info(calc);
        SaveToDatabase("BETWEEN-GAME METABOLISM DECAY", calc, "PlayerPoints");
        log.info("initialised successfully");
	}

	public override void Update()
	{
		if (Platform.Instance.LocalPlayerPosition == null || Platform.Instance.LocalPlayerPosition.IsTracking) return;  //TODO: should decay metabolism here
        
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
        double pointsDelta = (awardDistance * lastBaseMultiplierPercent * BASE_POINTS_PER_METRE) / 100.0f;
		_currentActivityPoints += pointsDelta;
		
		// increment metabolism
        float metabolismDelta = (float)Mathf.Exp(-(CurrentMetabolism-100)/20) // the more you have, the harder it is to earn
                /(60*BASE_MULTIPLIER_TIME_THRESH); // scale per-minute reward to trigger time of this loop
		_currentMetabolism += metabolismDelta;
            
        // check if we need to update the mutiplier
		if (basePointsSpeed != 0.0f && currentTime > lastMultiplierCheckTime + BASE_MULTIPLIER_TIME_THRESH*1000)
		{
			long multiplierTime = currentTime - lastMultiplierCheckTime; //ms
			double multiplierDistance = currentDistance - lastMultiplierCheckDistance; //metres
            float multiplierSpeed = (float)(multiplierDistance*1000/multiplierTime);
            if (multiplierSpeed > basePointsSpeed)
			{
                // bump up the multiplier (incremented by BASE_MULTIPLIER_PERCENT each time round this loop for BASE_MULTIPLIER_LEVELS)
                if (lastBaseMultiplierPercent <= (1+BASE_MULTIPLIER_LEVELS*BASE_MULTIPLIER_PERCENT))
				{
                    lastBaseMultiplierPercent += BASE_MULTIPLIER_PERCENT;
                    Platform.Instance.GetMonoBehavioursPartner().SendMessage("NewBaseMultiplier", lastBaseMultiplierPercent/100.0f);
                    log.info("New base multiplier: " + lastBaseMultiplierPercent + "%");
                }
            } 
			else if (lastBaseMultiplierPercent != 100) 
			{
                // drop multiplier to 1
                lastBaseMultiplierPercent = 100;
                Platform.Instance.GetMonoBehavioursPartner().SendMessage("NewBaseMultiplier", lastBaseMultiplierPercent / 100.0f);
                log.info("New base multiplier: " + lastBaseMultiplierPercent + "%");
            }
			lastMultiplierCheckTime = currentTime;
		}
		

        // save the points & metabolism to the database every BASE_MULTIPLIER_TIME_THRESH
		if (lastMultiplierCheckTime == currentTime && _currentActivityPoints > 0)
		{
			SaveToDatabase("BASE_POINTS","Period = 8s, Multiplier = " + lastBaseMultiplierPercent + "%", "PlayerPoints");
			log.info ("Saving to database: " + CurrentActivityPoints + "activity-points, " + OpeningPointsBalance + "opening-points, " + CurrentGemBalance + "gems and metabolism of " + CurrentMetabolism);
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
		t.points_delta = CurrentActivityPoints + OpeningPointsBalance - (lastTransaction == null ? 0 : lastTransaction.points_balance);
		t.points_balance = CurrentActivityPoints + _openingPointsBalance;
		t.gems_delta = CurrentGemBalance - (lastTransaction == null ? 0 : lastTransaction.gems_balance);
		t.gems_balance = CurrentGemBalance;
		t.metabolism_delta = CurrentMetabolism - (lastTransaction == null ? 0 : lastTransaction.metabolism_balance);
		t.metabolism_balance = CurrentMetabolism;
		t.transaction_type = transactionType;
		t.transaction_calc = transactionCalc;
		t.source_id = sourceId;
        log.info ("New transaction details:" +
        "\nsource_id = " + t.source_id +
        "\npoints_delta = " + t.points_delta +
        "\npoints_balance = " + t.points_balance +
        "\ngems_delta = " + t.gems_delta +
        "\ngems_balance = " + t.gems_balance +
        "\nmetabolism_delta = " + t.metabolism_delta +
        "\nmetabolism_balance = " + t.metabolism_balance +
        "\ntransaction_type = " + t.transaction_type +
        "\ntransaction_calc = " + t.transaction_calc +
        "\nsource_id = " + t.source_id +
        "\nts = " + t.ts +
//        "\n_id = " + t._id +
        "\ntransaction_id = " + t.transactionId +
        "\nuser_id = " + t.user_id +
        "\ndeleted_at = " + t.deleted_at +
        "\ndeviceId = " + t.deviceId +
        "\ndirty = " + t.dirty +
        "\nid = " + t.id);
		t.save(db);
		lastTransaction = t;
	}

}

