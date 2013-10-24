using UnityEngine;
using System.Collections;

public class FlowButton : MonoBehaviour 
{
    public Panel owner;
    public string name;

	public void OnClick()
    {
        if (owner != null)
        {
            owner.OnClick(this);
        }
    }

    public void OnPress(bool isDown)
    {
        if (owner != null)
        {
            owner.OnPress(this, isDown);
        }
    }
}
