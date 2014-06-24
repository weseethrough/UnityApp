using UnityEngine;
using System.Collections;

public class HideHomeIfNavHistory : MonoBehaviour {

	/// <summary>
    /// Should never have both home and back buttons.
    /// </summary>
	void Start () {
        bool backAllowed = FlowStateMachine.GetCurrentFlowState().parentMachine.IsBackAllowed();
        gameObject.GetComponentInChildren<UISprite>().enabled = !backAllowed;
	}
}
