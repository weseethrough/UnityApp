using UnityEngine;
using System.Collections;

public class BoltController : TargetController {
	
	private float speed = 10.44f;
	
	private float startDistance = 0.0f;
	
	public float distanceFromStart = 0.0f;
	
	double playerDistance = 0;
	
	private Animator anim;
	
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
	}
	
	void OnEnable()
	{
		startDistance = (float)Platform.Instance.Distance();
		distanceFromStart = startDistance;
		anim = GetComponent<Animator>();
		anim.SetFloat("Speed", speed);
		anim.speed = 1.5f;
		SetAttribs(0, 1, transform.position.y, transform.position.x);
	}
	
	void OnDisable()
	{
		anim.SetFloat("Speed", 0.0f);
	}
	
	// Update is called once per frame
	void Update () 
	{
		playerDistance = Platform.Instance.Distance();
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
		float relativeDist = distanceFromStart - (float)playerDistance;
		return relativeDist;
	}
	
	public float GetBoltDistanceTravelled()
	{
		return distanceFromStart - startDistance;
	}
	
	public float GetPlayerDistanceTravelled()
	{
		return (float)playerDistance - startDistance;
	}
}
