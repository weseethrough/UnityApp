using UnityEngine;
using System.Collections;

public class FirstRunGPSScreenController : SwipeListener {
	
	GestureHelper.OnTap taphandler = null;
	GestureHelper.OnSwipeLeft leftHandler = null;
	GestureHelper.OnSwipeRight rightHandler = null;
	
	bool canProceed = false;
	
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
		
		//assume no gps to begin with, so ghost progress dot for it
		PageIndicatorController paging = Component.FindObjectOfType(typeof(PageIndicatorController)) as PageIndicatorController;
		int currentPage = (int)DataVault.Get("currentPageIndex");
		paging.GhostProgressBeyondPage(currentPage);

		
	}
	
	// Update is called once per frame
	void Update () {
		//check gps
		if(Platform.Instance.HasLock() || Platform.Instance.IsIndoor())
		{
			if(!canProceed)
			{
				canProceed = true;
				DataVault.Set("FrGPS_navPrompt", "Swipe Forward to proceed");
				//grey the icons
				PageIndicatorController paging = Component.FindObjectOfType(typeof(PageIndicatorController)) as PageIndicatorController;
				if(paging)
				{
					paging.UnGhostAllPages();
				}
				else
				{	UnityEngine.Debug.LogWarning("FirstRunGPS: couldn't find paging controller"); }
				//update strings
				setStringsForIndoor(Platform.Instance.IsIndoor());
			}
		}
		else
		{
			if(canProceed)
			{
				canProceed = false;
				DataVault.Set("FrGPS_navPrompt", " ");
				//grey out the icons
				PageIndicatorController paging = Component.FindObjectOfType(typeof(PageIndicatorController)) as PageIndicatorController;
				if(paging)
				{
					int currentPage = (int)DataVault.Get("currentPageIndex");
					paging.GhostProgressBeyondPage(currentPage);
				}
				else
				{	UnityEngine.Debug.LogWarning("FirstRunGPS: couldn't find paging controller"); }
				//update strings
				setStringsForIndoor(Platform.Instance.IsIndoor());
			}
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
			DataVault.Set("FrGPS_tapPrompt", "Tap to switch to Demo Mode");
			//don't show swipe to proceed if no gps lock
			if(!Platform.Instance.HasLock())
			{
				DataVault.Set ("FrGPS_navPrompt", "");
				DataVault.Set("FrGPS_body", "Awaiting GPS\nTether Glass to a Phone and head outside to get a GPS lock");
			}
			else
			{
				DataVault.Set ("FrGPS_body", "GPS lock successfully acquired");
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
