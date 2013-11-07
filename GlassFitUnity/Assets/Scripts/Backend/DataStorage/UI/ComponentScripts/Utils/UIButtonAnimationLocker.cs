using UnityEngine;
using System.Collections;

public class UIButtonAnimationLocker : MonoBehaviour
{
    UIImageButton button;

    public void OnButtonAnimStarted()
    {
        if (button == null)
        {
            button = GetComponent<UIImageButton>();
        }

        if (button != null) button.isEnabled = false;
        Debug.Log("locked: " + button);
    }

    public void OnButtonAnimFinished()
    {
        if (button == null)
        {
            button = GetComponent<UIImageButton>();
        }

        if (button != null) button.isEnabled = true;
    }
}

