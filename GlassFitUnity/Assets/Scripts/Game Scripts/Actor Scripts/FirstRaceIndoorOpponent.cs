using UnityEngine;
using System.Collections;

public class FirstRaceIndoorOpponent : TargetController {
	
	private double playerDistance = 0f;
	private float playerSpeed = 0f;
	
	private float distanceFromStart = 0;
	
	private bool notVisible = true;
	
	private Animator anim;
	
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
		//renderer.enabled = false;
		SetAttribs(0.0f, 1.0f, transform.position.y, transform.position.x);
	}
	
	void OnEnable()
	{
		anim = GetComponent<Animator>();
		//renderer.enabled = false;
		SetAttribs(0.0f, 1.0f, transform.position.y, transform.position.x);
	}
	
	// Update is called once per frame
	void Update () {
		UnityEngine.Debug.Log("IndoorOpponent: we are in the update function");
		playerDistance = Platform.Instance.Distance();
		playerSpeed = Platform.Instance.Pace();
		
		SetAnimSpeed(playerSpeed);
		
		distanceFromStart += Time.deltaTime * playerSpeed;
		
		base.Update();
	}
	
	/// <summary>
	/// Override base implementation, which queries target tracker.
	/// </summary>
	/// <returns>
	/// The distance behind this target.
	/// </returns>
	public override double GetDistanceBehindTarget ()
	{
		float relativeDist = distanceFromStart - (float)playerDistance;
		UnityEngine.Debug.Log("IndoorOpponent: distance from start is " + distanceFromStart.ToString("f0") + " and player distance is " + playerDistance.ToString("f0"));
		return relativeDist;
	}
	
	public override void SetHeadstart(float dist) {
		//headStartDistance = dist;
		distanceFromStart = dist;	
	}
	
	void SetAnimSpeed(float speed)
	{
		//pick appropriate anim speed based on our movement speed.
		//UnityEngine.Debug.Log("FirstRace: speed is " + speed.ToString("f2"));
		anim.SetFloat("Speed", speed);
		if(speed > 2.2f && speed < 4.0f) {
			anim.speed = speed / 2.2f;
		} else if(speed > 4.0f) {
			anim.speed = Mathf.Clamp(speed / 4.0f, 1, 2);
		} else if(speed > 0) {
			anim.speed = speed / 1.25f;
		} else {
			anim.speed = 1f;
		}
	}
}
