using UnityEngine;
using System.Collections;

public class SoundController : TargetController {
	private const float NUM_TRACKS = 11;
	private AudioSource[] stevies;
	private float curTime = 0.0f;
	private string indoorText = "Indoor Active";
	private bool indoor = true;
	private float score = 0.0f;
	private int mult = 1;
	private int originalHeight = 500;
	private int originalWidth = 800;
	private Vector3 scale = Vector3.one;
	private bool countdown = false;
	private float countTime = 3.0f;
	private bool started = false;
	
	private int currentTrack = 1;
	
	// Use this for initialization
	void Start () {
		stevies = GetComponents<AudioSource>();
		scale.x = (float)Screen.width/originalWidth;
		scale.y = (float)Screen.height/originalHeight;
	}
	
	void OnGUI() {
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		if(GUI.Button(new Rect(originalWidth/2-100, originalHeight-100, 200, 100), indoorText))
		{
			if(indoor)
			{
				indoor = false;
				indoorText = "Outdoor Active";
				Platform.Instance.StopTrack();
				Platform.Instance.Reset();
				Platform.Instance.SetIndoor(indoor);
				//Platform.Instance.StartTrack(indoor);
				score = 0;
				mult = 1;
				curTime = 0.0f;
				countdown = false;
				started = false;
				countTime = 3.0f;
				currentTrack = 1;
				for(int i=1; i<NUM_TRACKS; i++)
				{
					stevies[i].volume = 0.0f;
				}
			}
			else
			{
				indoor = true;
				indoorText = "Indoor Active";
				Platform.Instance.StopTrack();
				Platform.Instance.Reset();
				Platform.Instance.SetIndoor(indoor);
				//Platform.Instance.StartTrack(indoor);
				score = 0;
				mult = 1;
				curTime = 0.0f;
				countdown = false;
				started = false;
				countTime = 3.0f;
				currentTrack = 1;
				for(int i=1; i<NUM_TRACKS; i++)
				{
					stevies[i].volume = 0.0f;
				}
			}
		}
		GUI.Label(new Rect(originalWidth/2-100, 0, 200, 100), "Current Score: " + score.ToString("f0"));
		
		if(countdown)
		{
			GUI.skin.label.fontSize = 40;
			int cur = Mathf.CeilToInt(countTime);
			if(countTime > 0.0f)
			{
				GUI.Label(new Rect(400, 200, 200, 200), cur.ToString()); 
			}
			else if(countTime > -1.0f && countTime < 0.0f)
			{
				GUI.Label(new Rect(400, 200, 200, 200), "GO!"); 
			}
		}
	}
	
	void Update () {
		
		Platform.Instance.Poll();
		
		if(Platform.Instance.HasLock() || indoor)
		{
			countdown = true;
		 	if(countTime <= -1.0f && !started)
			{
				Platform.Instance.StartTrack();
				UnityEngine.Debug.LogWarning("Tracking Started");
				started = true;
			}
			else if(countTime > -1.0f)
			{
				UnityEngine.Debug.LogWarning("Counting Down");
				countTime -= Time.deltaTime;
			}
		}
		
		double dist = Platform.Instance.DistanceBehindTarget(target) - 50;
		
		if(dist <= 0.0 && started)
		{
			curTime += Time.deltaTime;
			if(curTime > 3.0f)
			{
				curTime -= 3.0f;
				if(mult < 4)
				{
					mult++;
				}
				
				if(currentTrack < NUM_TRACKS)
				{
					stevies[currentTrack].volume = 1.0f;
					currentTrack++;
				}
			}
			score += Time.deltaTime * mult;
		}
		else if(started && dist > 0.0)
		{
			curTime -= Time.deltaTime;
			if(mult > 1)
			{
				mult = 1;
			}
			
			if(curTime < 0.0f && currentTrack > 0)
			{
				stevies[currentTrack].volume = 0.0f;
				curTime += 3.0f;
				currentTrack--;
			}
		}
		
		
		
	}
}
