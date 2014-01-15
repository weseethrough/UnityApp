using UnityEngine;
using System.Collections;

public class TrainController_Rescue : TargetController {
	
	protected float headStartDistance = 0.0f;
	protected float currentMovementSpeed = 7.0f;
	protected float timeRunStarted;
	protected float playerDistance = 0.0f;
	
	protected Vector3 absolutePos = Vector2.zero;
	protected bool isOnDetour = false;
	protected float detourDistTravelled = 0.0f;
	
	public const float DETOUR_DISTANCE = 200.0f;
	
	protected bool hasBegunRace = false;
	
	// Use this for initialization
	void Start () {
		travelSpeed = 1.0f;	//somewhat arbitrary scale factor for positioning distance
		lane = 1;
		lanePitch = 1.0f;
		//SetAttribs(0.0f, 1.0f, transform.position.y, transform.position.x);
		
		timeRunStarted = Time.time;
		xOffset = transform.localPosition.x;
		absolutePos = transform.localPosition;
	}
	
	public void BeginRace() {
		hasBegunRace = true;
	}
	
	// Update is called once per frame
	public override void Update () {
		
#if !UNITY_EDITOR
		if(!hasBegunRace)
		{
			return;
		}
#endif
		
		//move forward along track
		Vector3 direction = GetCurrentDirection();
		Vector3 deltaPos = Time.deltaTime * direction * currentMovementSpeed;
		absolutePos = absolutePos + deltaPos;
		
		//set orientation
		float angleRads = Mathf.Atan2(direction.x, direction.z);
		float angle = angleRads * Mathf.Rad2Deg;
		transform.localEulerAngles = new Vector3(0.0f, angle, 0.0f);
		
		if(isOnDetour)
		{
			detourDistTravelled += deltaPos.magnitude;
		
			//check if we've finished the detour
			if(detourDistTravelled > DETOUR_DISTANCE)
			{
				isOnDetour = false;
				//straighten us up to be exactly on the main track
				absolutePos.x = xOffset;
			}
		}
		
		//set position as relative to camera
		playerDistance = Platform.Instance.GetDistance();
		Vector3 playerPos = new Vector3(0,0,playerDistance);
		transform.localPosition = absolutePos - playerPos;
	}
	
	public void BeginDetour()
	{
		isOnDetour = true;
		detourDistTravelled = 0.0f;
	}
	
	public float GetForwardDistance()
	{
		return absolutePos.z;	
	}
	
	/// <summary>
	/// Gets the current direction.
	/// Eventually this should look up a spline or similar
	/// </summary>
	/// <returns>
	/// The current direction.
	/// </returns>j
	Vector3 GetCurrentDirection() {
		if(!isOnDetour)
		{
			return new Vector3(0.0f, 0.0f, 1.0f);	
		}
		else
		{
			float detourProgress = detourDistTravelled/DETOUR_DISTANCE;
			//on detour
			if(detourProgress < 0.33f)
			{
				//60 degrees away from straight forward
				return new Vector3(0.86602540378f, 0.0f, 0.5f);
			}
			else if(detourProgress < 0.66f)
			{
				return new Vector3(0,0,1);
			}
			else
			{
				//60 degrees towards straight foward
				return new Vector3(-0.86602540378f, 0.0f, 0.5f);
			}
		}
	}
	
	/// <summary>
	/// Override base implementation, which queries target tracker.
	/// </summary>
	/// <returns>
	/// The distance behind this target.
	/// </returns>
	public override double GetDistanceBehindTarget ()
	{
		float relativeDist = absolutePos.z - (float)playerDistance;
		return relativeDist;
	}
}
