using UnityEngine;
using System.Collections;

public class FlowButton : MonoBehaviour 
{
    private Panel m_owner;
    public Panel owner
    {
        get 
        { 
            Debug.Log("get panel owner: " + m_owner.GetDisplayName());
            return m_owner;
        }
        set 
        { 
            m_owner = value;
            Debug.Log("set panel owner: "+m_owner.GetDisplayName());
        }
    }
    public string name;

    void Start()
    {
        Debug.Log("Flow button creation: " + gameObject.name);
    }

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
