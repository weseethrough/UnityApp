using UnityEngine;
using System.Collections;
using RaceYourself;

public class TrainController_Rescue : WorldObject {
	
	protected float headStartDistance = 0.0f;
	protected float currentMovementSpeed = 2.4f;
	protected float timeRunStarted;
	protected float playerDistance = 0.0f;
	
	protected bool hasBegunRace = false;

	protected bool bSoundedHorn = false;
	
	protected AudioSource hornSound;
	protected AudioSource wheelSound;
	protected AudioSource bellSound;
	
	private int trainLevel = 0;
	
	private double playerStartDistance;
	
	// Use this for initialization
	public override void Start () {
		base.Start();

		timeRunStarted = Time.time;

		Component[] sources = GetComponents(typeof(AudioSource));
		wheelSound = (AudioSource)sources[0];
		hornSound = (AudioSource)sources[1];
		bellSound = (AudioSource)sources[2];
	}

	
	public void BeginRace() {
		hasBegunRace = true;
		//sound the bell
		hornSound.Play();
		
		playerStartDistance = Platform.Instance.LocalPlayerPosition.Distance;

		//enable the on-look-at audio
		OnLookAtSound lookAtSound = GetComponent<OnLookAtSound>();
		lookAtSound.enabled = true;
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();
	}
	
	public void soundBell()
	{
		if(bellSound != null)
		{
			bellSound.Play();
		}
	}
		
	public float GetForwardDistance()
	{
		return getRealWorldPos().z;
	}
	


}
