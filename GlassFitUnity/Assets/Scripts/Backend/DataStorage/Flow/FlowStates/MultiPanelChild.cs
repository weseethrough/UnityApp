using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

/// <summary>
/// basic panel which allows to show ui
/// </summary>
[Serializable]
public class MultiPanelChild : Panel
{    
    /// <summary>
    /// default constructor
    /// </summary>
    /// <returns></returns>
    public MultiPanelChild() : base() { }

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public MultiPanelChild(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
	{
    }

    /// <summary>
    /// serialization function called by serializer
    /// </summary>
    /// <param name="info">serialziation info where all data would be pushed to</param>
    /// <param name="ctxt">serialzation context</param>
    /// <returns></returns>
    public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        base.GetObjectData(info, ctxt);
   	}

    /// <summary>
    /// Gets display name of the node, helps with node identification in editor
    /// </summary>
    /// <returns>name of the node</returns>
    public override string GetDisplayName()
    {
        base.GetDisplayName();
        
        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return "MultiPanel: " + gName.Value;
        }
        return "MultiPanel: UnInitialzied";
    }
    
    /// <summary>
    /// Normal Enter/Exit behaviors have been overridden. Multipanel is managed by its parent not by himself
    /// </summary>
    /// <returns></returns>
    public override void EnterStart() 
    {
        m_state = State.Entering;             
    }

    /// <summary>
    /// Normal Enter/Exit behaviors have been overridden. Multipanel is managed by its parent not by himself
    /// </summary>
    /// <returns>returns always true</returns>
    public override bool EnterUpdate() { return true; }

    /// <summary>
    /// Normal Enter/Exit behaviors have been overridden. Multipanel is managed by its parent not by himself
    /// </summary>
    /// <returns></returns>
    public override void Entered() 
    {
        m_state = State.Idle;
    }

    /// <summary>
    /// Normal Enter/Exit behaviors have been overridden. Multipanel is managed by its parent not by himself
    /// </summary>
    /// <returns></returns>
    public override void ExitStart() 
    {
        m_state = State.Exiting;
    }

    /// <summary>
    /// Normal Enter/Exit behaviors have been overridden. Multipanel is managed by its parent not by himself
    /// </summary>
    /// <returns>returns always true</returns>
    public override bool ExitUpdate() { return true; }

    /// <summary>
    /// Normal Enter/Exit behaviors have been overridden. Multipanel is managed by its parent not by himself
    /// </summary>
    /// <returns></returns>
    public override void Exited() 
    {
        m_state = State.Dead;
    }

    /// <summary>
    /// Method called by parent to prepare state for use.
    /// </summary>
    /// <returns></returns>
    public void ManagedEnter()
    {
        base.EnterStart();
        base.Entered();
    }

    /// <summary>
    /// Method which do all required exit operations when parent state is leaving
    /// </summary>
    /// <returns></returns>
    public void ManagedExit()
    {
        base.ExitStart();
        base.Exited();
    }

}
