using UnityEngine;
using System.Collections;

public class MobileLoginSubmit : MonoBehaviour {

    public void OnSubmit ()
    {
        FlowState fs = FlowStateMachine.GetCurrentFlowState();
        Panel panel = (Panel) fs;
        GConnector gc = panel.Outputs.Find(r => r.Name == "Login");
        panel.parentMachine.FollowConnection(gc);
        ButtonFunctionCollection.AllowLogin(null, fs);
    }
}
