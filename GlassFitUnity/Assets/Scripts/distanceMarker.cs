using UnityEngine;
using System.Collections;

public class distanceMarker : MonoBehaviour {
	
	private Platform inputData = null;

	private int target = 50;
	
	//GameObjects for each marker
	public GameObject fiftyMarker;
	public GameObject fiftyMarker2;
	
	private float countTime = 3.99f;
	private bool countdown = false;
	private bool started = false;
	
	public GameObject text1;
	public GameObject text2;
	
	private TextMesh t1;
	private TextMesh t2;

	// Boxes to check distance
	private double distance;
	private Rect distanceBox;
	
	void Start () 
	{	
		inputData = new Platform();

		
		t1 = text1.GetComponent<TextMesh>();
		t2 = text2.GetComponent<TextMesh>();
		
		distanceBox = new Rect(Screen.width/2, Screen.height - 50, 50, 50);
		//inputData.StartTrack(false);
	}
	
	void ResetMarkers()
	{
		fiftyMarker.transform.position = new Vector3(0, 0, 5000);
		fiftyMarker2.transform.position = new Vector3(0, 0, 5000);
	}
	
	void Update () 
	{
		
//		if(!countdown)
//		{
//			if(inputData.hasLock())
//			{
//				countdown = true;
//			}
//		}
//		else
//		{
//			if(!started)
//			{
//				inputData.StartTrack(false);
//				started = true;
//			}
//			countTime -= Time.deltaTime;
//		}
			
		if(countTime == 3.99f && inputData.hasLock() && !started)
		{
			started = true;
		}
		
		if(started && countTime <= 0.0f)
		{
			inputData.StartTrack(false);
		}
		else if(started && countTime > 0.0f)
		{
			countTime -= Time.deltaTime;
		}
		
		inputData.Poll();
		distance = inputData.Distance();
		
		ResetMarkers();
		
//		if(!started && Input.touchCount == 3)
//		{
//			started = true;
//			inputData.Start(false);
//		}
		
		// 50m markers
		if(distance > target - 20 && distance < target + 20)
		{
			double deltDist = target - distance;
			deltDist *= 6.666f;
			fiftyMarker.transform.position = new Vector3(15, 0, (float)deltDist);
			fiftyMarker2.transform.position = new Vector3(-15, 0, (float)deltDist);
		}
		
		if(distance > target + 20) 
		{
			target +=50;
			t1.text = target.ToString();
			t2.text = target.ToString();
		}
	}
}
