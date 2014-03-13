using System;
using UnityEngine;

public abstract class PlayerPoints
{
	protected Log log = new Log("PlayerPoints");  // for use by subclasses

	public abstract long CurrentActivityPoints { get; }

	public abstract long OpeningPointsBalance { get; }

	public abstract int CurrentGemBalance { get; }

	public abstract float CurrentMetabolism { get; }

	public abstract void Update();

	public abstract void Reset();

	public abstract void SetBasePointsSpeed(float speed);

	public abstract void AwardPoints(String reason, String gameId, long points);

	public abstract void AwardGems(string reason, string gameId, int gems);

}

