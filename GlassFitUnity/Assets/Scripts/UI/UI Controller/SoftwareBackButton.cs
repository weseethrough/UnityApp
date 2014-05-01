using UnityEngine;
using System.Collections;

public class SoftwareBackButton : UIComponentSettings {

    void Awake () {
        DataVault.RegisterListner(this, "transition_ended");
        Apply ();
	}

    public override void Apply()
    {
        if(Platform.Instance.ProvidesBackButton() || !FlowStateMachine.GetCurrentFlowState().parentMachine.IsBackAllowed())
        {
            UnityEngine.Debug.Log("SoftwareBackButton: disabling software back button");
            gameObject.SetActive(false);
        }
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
