using UnityEngine;
using System.Collections;

public class FirstRunGPSScreenController : SwipeListener {
	
	GestureHelper.OnTap taphandler = null;
	GestureHelper.OnSwipeLeft leftHandler = null;
	GestureHelper.OnSwipeRight rightHandler = null;
	
	// Use this for initialization
	public void Start () {
		
		base.Start();

		UnityEngine.Debug.Log("Starting FirstRun GPS screen controller");
		
		//register tap handler
		taphandler = new GestureHelper.OnTap( () => {
			HandleTap();
		});
		GestureHelper.onTap += taphandler;
	
		bool bIndoor = Platform.Instance.IsIndoor();
		setStringsForIndoor(bIndoor);
		
	}
	
	// Update is called once per frame
	void Update () {
		//check gps
		if(Platform.Instance.HasLock() || Platform.Instance.IsIndoor())
		{
			DataVault.Set("FrGPS_navPrompt", "Swipe Forward to proceed");
		}
		else
		{
			DataVault.Set("FrGPS_navPrompt", " ");
		}
	}
	
	public void HandleTap() {
		//toggle indoor mode
		bool bIndoor = Platform.Instance.IsIndoor();
		Platform.Instance.SetIndoor(!bIndoor);
		//update strings as appropriate
		setStringsForIndoor(!bIndoor);
	}
	
	protected override void handleRight ()
	{
		bool canProceed = Platform.Instance.IsIndoor() || Platform.Instance.HasLock();
		if(canProceed)
		{
			base.handleRight ();
		}
	}

	void setStringsForIndoor(bool indoor) {
		if(indoor)
		{
			DataVault.Set("FrGPS_title", "Demo Mode");
			DataVault.Set("FrGPS_body", "Demo Mode works indoors without GPS\nRun on the spot to move forwards");
			DataVault.Set("FrGPS_tapPrompt", "Tap to switch to Normal Mode");
			DataVault.Set ("FrGPS_navPrompt", "Swipe Forward to proceed");
		}
		else
		{
			//outoodr
			DataVault.Set("FrGPS_title", "GPS Lock Required");
			DataVault.Set("FrGPS_body", "Tether Glass to a Phone and head outside to get a GPS lock");
			DataVault.Set("FrGPS_tapPrompt", "Tap to switch to Demo Mode");
			//don't show swipe to proceed if no gps lock
			if(!Platform.Instance.HasLock())
			{
				DataVault.Set ("FrGPS_navPrompt", "");
			}
		}
	}
	
	void OnDestroy()
	{
		base.OnDestroy();
		//unregister tap handler
		GestureHelper.onTap -= taphandler;
	}
}
