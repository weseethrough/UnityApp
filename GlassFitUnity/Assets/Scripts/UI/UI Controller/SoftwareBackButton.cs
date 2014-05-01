using UnityEngine;
using System.Collections;

public class SoftwareBackButton : UIComponentSettings {

    void Awake () {
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
            UnityEngine.Debug.Log("SoftwareBackButton: disabling software back button");
            gameObject.SetActive(false);
        }
        // Don't enable the back button mid-transition (to avoid a transition between two screens without back buttons flashing up the back button).
        else if (!((bool) DataVault.Get("transition_ended")))
        {
            UnityEngine.Debug.Log("SoftwareBackButton: enabling software back button");
            gameObject.SetActive(true);
        }
    }

	public void OnClick ()
    {
		//treat as a back gesture
		GestureHelper.onBack();
	}
}
