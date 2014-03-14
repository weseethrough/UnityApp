using UnityEngine;
using System.Collections;

public class HazardSnack : SnackBase {
	
	GestureHelper.ThreeFingerTap threeHandler = null;
	
	private double distanceTravelled = 0;
	
	private double startDistance = 0;
	
	// Use this for initialization
	void Start () {
	
	}
	
	public override void Begin() {
		base.Begin();
		
		SetTrack(false);
		
		SetMainCamera(false);
		
		threeHandler = new GestureHelper.ThreeFingerTap(() => {
			Finish();
		});
		
		startDistance = Platform.Instance.LocalPlayerPosition.Distance;
		
		GestureHelper.onThreeTap += threeHandler;
		
		SetThirdPerson(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnDestroy()
	{
		GestureHelper.onThreeTap -= threeHandler;
	}
	
	public void EndGame()
	{
		finish = true;
		distanceTravelled = Platform.Instance.LocalPlayerPosition.Distance - startDistance;
		DataVault.Set("death_colour", "EA0000FF");
		DataVault.Set("snack_result", "You died!");
		DataVault.Set("snack_result_desc", "You reached " + distanceTravelled.ToString("f0") + "m!");
		StartCoroutine(ShowBanner(3.0f));
	}
}
