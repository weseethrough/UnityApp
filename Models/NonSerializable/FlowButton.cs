using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// base component for all flow buttons, injected by panels to the buttons handles their internal events and forwards them back to the parent class
/// </summary>
public class FlowButton : MonoBehaviour 
{
    private FlowStateBase m_owner;
    public FlowStateBase owner
    {
        get 
        {             
            return m_owner;
        }
        set 
        { 
            m_owner = value;          
        }
    }
    public new string name;
    public Dictionary<string, System.Object> userData = new Dictionary<string,object>();

	/// <summary>
	/// Event called by the button when it were clicked (pressed and then released). Sends message to the parent about it
	/// </summary>
	/// <returns></returns>
	public void OnClick()
    {
        /*if (transform != null && transform.parent != null && transform.parent.parent != null)
        {
            Debug.Log("CLICK on button: " + transform.name + " -> " + transform.parent.name + " -> " + transform.parent.parent.name);
        }*/

        //check type of click
        if (Input.touchCount > 1)
        {
            //two fingers click are not click event for us
            return;
        }

        if (owner != null && (owner is PanelBase))
        {
            (owner as PanelBase).OnClick(this);
        }
    }

    /// <summary>
    /// Event called by the button when it were pressed.
    /// </summary>
    /// <param name="isDown">if button were just pressed it will contain value true, if released it will be false</param>
    /// <returns></returns>
    public void OnPress(bool isDown)
    {
        if (owner != null && (owner is PanelBase))
        {
            (owner as PanelBase).OnPress(this, isDown);
        }
    }
}
