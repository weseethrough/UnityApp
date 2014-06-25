﻿using UnityEngine;
using System.Collections;

public class SoftwareBackButton : UIComponentSettings {

    private static Log log = new Log("SoftwareBackButton");

    void Awake () {
        // Check whether to display the back button or not when the transition to a new screen has ended.
        DataVault.RegisterListner(this, "transition_ended");
        Apply ();
	}

    public override void Apply()
    {
        base.Apply();
        // We don't want to put a back button on-screen if the device already provides one (OS-provided soft button or hard button)
        // Furthermore, we want to check whether transitioning backwards is allowed for this screen.
        if(Platform.Instance.ProvidesBackButton() || !FlowStateMachine.GetCurrentFlowState().parentMachine.IsBackAllowed())
        {
            log.info("SoftwareBackButton: disabling software back button");
            gameObject.SetActive(false);
        }
        // Don't enable the back button mid-transition (to avoid a transition between two screens without back buttons flashing up the back button).
        else if (!((bool) DataVault.Get("transition_ended")))
        {
            log.info("SoftwareBackButton: enabling software back button");
            gameObject.SetActive(true);
        }
    }

	public void OnClick ()
    {
        //treat as a back gesture
		GestureHelper.onBack();
	}
}
