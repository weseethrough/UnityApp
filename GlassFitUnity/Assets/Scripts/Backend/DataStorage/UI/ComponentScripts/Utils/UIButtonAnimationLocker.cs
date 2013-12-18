using UnityEngine;
using System.Collections;

/// <summary>
/// component allowing to track current state of the animation and send feedback callbacks
/// </summary>
public class UIButtonAnimationLocker : MonoBehaviour
{
    public delegate void CallbackAnimFinished(UIImageButton button);

    UIImageButton button;    
    CallbackAnimFinished callbackFunction;

    public bool locked = true;

    public UIButtonAnimationLocker.CallbackAnimFinished CallbackFunction
    {
        get { return callbackFunction; }
        set { callbackFunction = value; }
    }


    /// <summary>
    /// Function called when button animation starts to block button functionality for this time
    /// </summary>
    /// <returns></returns>
    public void OnButtonAnimStarted()
    {
        locked = true;
        if (button == null)
        {
            button = GetComponent<UIImageButton>();
        }

        if (button != null) button.isEnabled = false;
    }

    /// <summary>
    /// animation finished in which case button gets enabled again 
    /// </summary>
    /// <returns></returns>
    public void OnButtonAnimFinished()
    {
        locked = false;

        if (button == null)
        {
            button = GetComponent<UIImageButton>();
        }

        if (button != null) button.isEnabled = true;

        /*if (callbackFunction != null)
        {
            callbackFunction(button);
        }*/
    }
}

