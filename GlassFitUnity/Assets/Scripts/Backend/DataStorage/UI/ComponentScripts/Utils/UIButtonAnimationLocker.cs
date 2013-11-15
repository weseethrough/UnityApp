using UnityEngine;
using System.Collections;

public class UIButtonAnimationLocker : MonoBehaviour
{
    public delegate void CallbackAnimFinished(UIImageButton button);

    UIImageButton button;    
    CallbackAnimFinished callbackFunction;

    public UIButtonAnimationLocker.CallbackAnimFinished CallbackFunction
    {
        get { return callbackFunction; }
        set { callbackFunction = value; }
    }


    public void OnButtonAnimStarted()
    {
        if (button == null)
        {
            button = GetComponent<UIImageButton>();
        }

        if (button != null) button.isEnabled = false;
    }

    public void OnButtonAnimFinished()
    {
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

