using System;

public class EditorPlayerPoints : PlayerPoints
{

	public override long CurrentActivityPoints { get { return (long)(Platform.Instance.LocalPlayerPosition.Distance * 5); } }

	public override long OpeningPointsBalance { get { return 20000; } }

	public override int CurrentGemBalance { get { return 100; } }

	public override float CurrentMetabolism	{ get { return 1.0f; } }

	public override void Update() {}

	public override void Reset() {}

	public override void SetBasePointsSpeed (float speed)
	{
		//do nothing
		return;
	}
	
	public override void AwardPoints (string reason, string gameId, long points)
	{
		//do nothing
		return;
	}
	
	public override void AwardGems (string reason, string gameId, int gems)
	{
		//do nothing
		return;
	}

}

