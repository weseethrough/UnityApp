using UnityEngine;
using System.Collections;

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

    void Start()
    {
       // Debug.Log("Flow button creation: " + gameObject.name);
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
