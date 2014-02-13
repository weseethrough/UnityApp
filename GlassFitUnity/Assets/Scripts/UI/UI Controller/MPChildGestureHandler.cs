using UnityEngine;
using System.Collections;

/// <summary>
/// MP child gesture handler.
/// Base class for any scripts which need to run within a multipanel child.
/// Gestures will only be handled if the child is in focus
/// Currently only supports tap, 2-tap, but more can easily be supported
/// </summary>
public class MPChildGestureHandler : MonoBehaviour {
	
	GestureHelper.OnTap handleTap;
	GestureHelper.TwoFingerTap handleTwoTap;
	
	// Use this for initialization
	public virtual void Start () {
		handleTap = new GestureHelper.OnTap( () => {
			if(IsInFocus())
			{
				HandleTap();
			}
		} );
		GestureHelper.onTap += handleTap;
		
		handleTwoTap = new GestureHelper.TwoFingerTap( () => {
			if(IsInFocus())
			{
				HandleTwoTap();
			}
		} );
		GestureHelper.onTwoTap += handleTwoTap;
		
		UnityEngine.Debug.Log("MPGestureHandler: Created gesture listeners");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	protected bool IsInFocus () {
		
		UnityEngine.Debug.Log("MPGestureHandler: Checking whether we are in focus");
		MultiPanel parentPanel = FlowStateMachine.GetCurrentFlowState() as MultiPanel;
		if(parentPanel != null)
		{
			UnityEngine.Debug.Log("MPGestureHandler: Found parent panel");
			MultiPanelChild focusedChild = parentPanel.GetFocusedChild();
			//check if this script is in the focused child
			if( IsInMultiChildPanel(focusedChild) )
			{
				return true;
			}
		}
		return false;
	}
	
	
	protected bool IsInMultiChildPanel(MultiPanelChild panel)
	{
		if(panel != null)
			{
				//UnityEngine.Debug.Log("MPGestureHandler: Found focused child");
				//if this script is within that child, we are in focus
				MPChildGestureHandler script = panel.physicalWidgetRoot.GetComponentInChildren<MPChildGestureHandler>();	
				if (script == this)
				{
					//UnityEngine.Debug.Log("MPGestureHandler: tested child was us. We are in focus");
					return true;
				}
				else if (script == null)
				{
					//UnityEngine.Debug.Log("MPGestureHandler: tested child doesn't have gesture listener component in children");
				}
				else
				{
					//UnityEngine.Debug.Log("MPGestureHandler: tested child isn't us");
				}
			}
		
		return false;
		
	}
	
	
	protected virtual void HandleTap() {
		UnityEngine.Debug.Log("Multipanel Child: handle tap");
	}
	
	protected virtual void HandleTwoTap() {
		UnityEngine.Debug.Log("Multipanel Child: handle two-tap");
	}
			
	void OnDestroy() {
		GestureHelper.onTap -= handleTap;
		GestureHelper.onTwoTap -= handleTwoTap;
	}
}
