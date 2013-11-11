using UnityEngine;
using System.Collections;

//used to forward some settings deeper into structure keeping it clean as a prefab
public class UIComponentSettings : MonoBehaviour 
{		
    
    /// <summary>
    /// This function is called every time we want to update component registered attributes
    /// </summary>
    /// <returns></returns>
    virtual public void Apply()
    {

    }

    /// <summary>
    /// This function is called once at the beginning of script live to ensure everything is ready and can respond to registrar call. 
    /// //For example this is the moment component might register for database events
    /// </summary>
    /// <returns></returns>
    virtual public void Register()
    {

    }

    protected virtual void OnDestroy()
    {
        DataVault.UnRegisterListner(this);
    }
}

