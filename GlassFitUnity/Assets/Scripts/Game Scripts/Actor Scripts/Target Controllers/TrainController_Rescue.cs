using UnityEngine;
using System.Collections;

public class TrainController_Rescue : TargetController {
	
	protected float headStartDistance = 0.0f;
	protected float currentMovementSpeed = 2.4f;
	protected float timeRunStarted;
	protected float playerDistance = 0.0f;
	
	protected Vector3 absolutePos = Vector2.zero;
	protected bool isOnDetour = false;
	protected float detourDistTravelled = 0.0f;
	
	public const float DETOUR_DISTANCE = 300.0f;
	
	protected bool hasBegunRace = false;
	protected float detourAngle = 0.0f;
	protected float detourTurnRate = 5.0f;	//5 degrees per second.
	
	protected bool bSoundedHorn = false;
	
	protected AudioSource hornSound;
	protected AudioSource wheelSound;
	protected AudioSource bellSound;
	
	private int trainLevel = 0;
	
	private double playerStartDistance;
	
	// Use this for initialization
	void Start () {
		travelSpeed = 1.0f;	//somewhat arbitrary scale factor for positioning distance
		lane = 1;
		lanePitch = 1.0f;
		//SetAttribs(0.0f, 1.0f, transform.position.y, transform.position.x);
		
		timeRunStarted = Time.time;
		xOffset = transform.localPosition.x;
		absolutePos = transform.localPosition;
		
		Component[] sources = GetComponents(typeof(AudioSource));
		wheelSound = (AudioSource)sources[0];
		hornSound = (AudioSource)sources[1];
		bellSound = (AudioSource)sources[2];
	}
	
	public void SetLevel(int level)
	{
		trainLevel = level;
		currentMovementSpeed = 2.4f + (trainLevel * 0.5f);
		UnityEngine.Debug.Log("TrainController: level is " + trainLevel.ToString());
	}
	
	public void BeginRace() {
		hasBegunRace = true;
		//sound the bell
		hornSound.Play();
		
		//absolutePos.z += (float)Platform.Instance.Distance();
		playerStartDistance = Platform.Instance.Distance();
		//enable the on-look-at audio
		OnLookAtSound lookAtSound = GetComponent<OnLookAtSound>();
		lookAtSound.enabled = true;
	}
	
	// Update is called once per frame
	public override void Update () {
		
		if(!hasBegunRace)
		{
			return;
		}
		
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
	
	public void soundBell()
	{
		if(bellSound != null)
		{
			bellSound.Play();
		}
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
			if(detourProgress < 0.1f)
			{	
				//turn towards 60 deg
				detourAngle = 600.0f * detourProgress;
			}
			else if(detourProgress < 0.3f)
			{
				//straight
				detourAngle = 60.0f;
			}
			else if(detourProgress < 0.4f)
			{	
				//turn towards 0 deg
				detourAngle = 600.0f * (0.4f-detourProgress);
			}
			else if(detourProgress < 0.6f)
			{
				//straight
				detourAngle = 0.0f;
			}
			else if(detourProgress < 0.7f)
			{
				//turn towards -60deg
				detourAngle = -600.0f * (detourProgress - 0.6f);
				
				//sound the horn, if we haven't
				if(!bSoundedHorn)
				{
					hornSound.Play();
				}
			}
			else if(detourProgress < 0.9f)
			{
				//straight
				detourAngle = -60.0f;
			}
			else if(detourProgress < 1.0f)
			{
				//turn towards 0 deg
				detourAngle = -600.0f * (1.0f - detourProgress);
			}
			else
			{
				//detour over
				isOnDetour = false;
			}
			
			//calculate vector
			detourAngle *= Mathf.Deg2Rad;
			return new Vector3(Mathf.Sin(detourAngle), 0.0f, Mathf.Cos(detourAngle));
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
		float relativeDist = absolutePos.z - ((float)playerDistance - (float)playerStartDistance);
		return relativeDist;
	}
}
