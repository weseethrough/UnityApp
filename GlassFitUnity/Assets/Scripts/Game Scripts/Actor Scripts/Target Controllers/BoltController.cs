using UnityEngine;
using System.Collections;

public class BoltController : TargetController {
	
	// Bolt's world record speed
	private float speed = 10.44f;
	
	// Distance where Bolt and the player started running
	private float startDistance = 0.0f;
	
	// Distance Bolt has travelled
	public float distanceFromStart = 0.0f;
	
	// Distance the player has travelled
	double playerDistance = 0;
	
	// Animator for the model
	private Animator anim;
	
	// Use this for initialization
	void Start () {
		// Get the animator
		anim = GetComponent<Animator>();
	}
	
	void OnEnable()
	{
		// Set the start distance
		startDistance = (float)Platform.Instance.Distance();
		distanceFromStart = startDistance;
		// Get the animator and set the speed
		anim = GetComponent<Animator>();
		anim.SetFloat("Speed", speed);
		anim.speed = 1.5f;
		// Set the atttributes for the character
		SetAttribs(0, 1, transform.position.y, transform.position.x);
	}
	
	void OnDisable()
	{
		// Make the model stop running
		anim.SetFloat("Speed", 0.0f);
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Get the player's distance
		playerDistance = Platform.Instance.Distance();
		// Calculate the distance travelled for Bolt
		distanceFromStart += Time.deltaTime * speed;
		
		base.Update();
	}
	
	/// <summary>
	/// Override base implementation, which queries target tracker.
	/// </summary>
	/// <returns>
	/// The distance behind this target.
	/// </returns>
	public override double GetDistanceBehindTarget()
	{
		// Calculate how far ahead/behind the player is
		float relativeDist = distanceFromStart - (float)playerDistance;
		return relativeDist;
	}
	
	/// <summary>
	/// Gets the distance travelled for Bolt
	/// </summary>
	/// <returns>
	/// The distance in meters
	/// </returns>
	public float GetBoltDistanceTravelled()
	{
		return distanceFromStart - startDistance;
	}
	
	/// <summary>
	/// Gets the player's distance travelled.
	/// </summary>
	/// <returns>
	/// The distance travelled.
	/// </returns>
	public float GetPlayerDistanceTravelled()
	{
		return (float)playerDistance - startDistance;
	}
}
