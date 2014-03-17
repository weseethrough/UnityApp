using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FirstRunGPSScreenController : MPChildGestureHandler {
	
	bool canProceed = false;
	
	MultiPanel parentPanel = null;
	MultiPanelChild containerMultipanelChild = null;
	int indexInParentPanel = -1;
	
	// Use this for initialization
	
	public override void Start() {
		base.Start();
	
		UnityEngine.Debug.Log("Starting FirstRun GPS screen controller");
	
		bool bIndoor = Platform.Instance.LocalPlayerPosition.IsIndoor();
		setStringsForIndoor(bIndoor);
		
		
		//get parent panel
		parentPanel = FlowStateMachine.GetCurrentFlowState() as MultiPanel;
		//get list of children
		List<MultiPanelChild> children = parentPanel.GetMultiPanelChildren();
		
		//find index which contains this script
		for(int i=0; i<children.Count; i++)
		{
			MultiPanelChild child = children[i];
			if( IsInMultiChildPanel(child) )
			{
				containerMultipanelChild = child;
				indexInParentPanel = i;
				break;
			}
		}
			
		//assume no gps to begin with, so ghost progress dot for it
		SetBlockProgress(true);
		
	}
	


	/// <summary>
	/// Sets whether progress should be blocked beyond this slide in the the multipanel.
	/// </summary>
	/// <param name='bBlock'>
	/// B block.
	/// </param>
	protected void SetBlockProgress(bool bBlock)
	{
		//set whether we should block
		
		//ghost or unghost
		PageIndicatorController paging = Component.FindObjectOfType(typeof(PageIndicatorController)) as PageIndicatorController;
		if(bBlock)
		{
			paging.GhostProgressBeyondPage(indexInParentPanel);
			parentPanel.SetLastAvaliableChild(containerMultipanelChild);
		}
		else
		{
			paging.UnGhostAllPages();
			parentPanel.SetLastAvaliableChild(null);
		}
		
	}
	
	
	
	// Update is called once per frame
	void Update () {
		//check gps
		if(Platform.Instance.LocalPlayerPosition.HasLock() || Platform.Instance.LocalPlayerPosition.IsIndoor())
		{
			if(!canProceed)
			{
				canProceed = true;
				DataVault.Set("FrGPS_navPrompt", "Swipe Forward to proceed");
				
				//allow swiping progress
				SetBlockProgress(false);
				
				//update strings
				setStringsForIndoor(Platform.Instance.LocalPlayerPosition.IsIndoor());
			}
		}
		else
		{
			if(canProceed)
			{
				canProceed = false;
				DataVault.Set("FrGPS_navPrompt", " ");
				
				//block swiping progress
				SetBlockProgress(true);
				
				//update strings
				setStringsForIndoor(Platform.Instance.LocalPlayerPosition.IsIndoor());
			}
		}
		
	}
	
	protected override void HandleTap() {
		//toggle indoor mode
		bool bIndoor = Platform.Instance.LocalPlayerPosition.IsIndoor();
		Platform.Instance.LocalPlayerPosition.SetIndoor(!bIndoor);

		///get new state and set strings as appropriate
		bIndoor = Platform.Instance.LocalPlayerPosition.IsIndoor();
		setStringsForIndoor(bIndoor);
	}

	void setStringsForIndoor(bool indoor) {
		if(indoor)
		{
			DataVault.Set("FrGPS_title", "Demo Mode");
			DataVault.Set("FrGPS_tapPrompt", "Tap to switch to Normal Mode\nor\nSwipe to continue");
			DataVault.Set("FrGPS_tether", "Demo Mode works indoors\nRun on the spot to move forwards");
			//set display condition
			DataVault.Set("FrGPS_ShowIcons", false);
		}
		else
		{
			//outdoor
			DataVault.Set("FrGPS_title", "GPS Lock Required");
			//set display condition to show icons
			DataVault.Set("FrGPS_ShowIcons", true);
			
			//don't show swipe to proceed if no gps lock
			if(!Platform.Instance.LocalPlayerPosition.HasLock())
			{	
				//please tether
				DataVault.Set("FrGPS_tether", "Tether phone to proceed");
				DataVault.Set("FrGPS_tapPrompt", "Tap to switch to Demo Mode\n");
				DataVault.Set("FrGPS_haveGPS", false);
			}
			else
			{
				//ready to go!
				DataVault.Set("FrGPS_tether", " ");
				DataVault.Set("FrGPS_tapPrompt", "GPS lock obtained\nSwipe to continue");
				DataVault.Set("FrGPS_haveGPS", true);
			}
		}
	}

}
