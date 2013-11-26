using UnityEngine;
using System.Collections;

/// <summary>
/// base component for all flow buttons, injected by panels to the buttons handles their internal events and forwards them back to the parent class
/// </summary>
public class FlowButton : MonoBehaviour 
{
    private Panel m_owner;
    public Panel owner
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
    public string name;

	/// <summary>
	/// Event called by the button when it were clicked (pressed and then released). Sends message to the parent about it
	/// </summary>
	/// <returns></returns>
	public void OnClick()
    {
        if (owner != null)
        {
            owner.OnClick(this);
        }
    }

    /// <summary>
    /// Event called by the button when it were pressed.
    /// </summary>
    /// <param name="isDown">if button were just pressed it will contain value true, if released it will be false</param>
    /// <returns></returns>
    public void OnPress(bool isDown)
    {
        if (owner != null)
        {
            owner.OnPress(this, isDown);
        }
    }
}
