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
public abstract class PanelBase : FlowStateBase
{
    static public string[] InteractivePrefabs = { "UIComponents/Button",
												  "MainGUIComponents/ResetGyroButton",
												  "MainGUIComponents/SettingsButton",
												  "SettingsComponent/IndoorButton",
												  "Friend List/ChallengeButton"};    
    public FlowPanelComponent panelNodeData;

    [NonSerialized()] public GameObject physicalWidgetRoot;    

    /// <summary>
    /// default constructor
    /// </summary>
    /// <returns></returns>
	public PanelBase() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public PanelBase(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
	{
        this.panelNodeData = (FlowPanelComponent)info.GetValue("panelNodeData", typeof(FlowPanelComponent));
        if (this.panelNodeData != null)
        {
            this.panelNodeData.RefreshData();
        }        
        
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
        info.AddValue("panelNodeData", this.panelNodeData);        
   	}

    public virtual void OnClick(FlowButton button)
    {

    }

    public virtual void OnPress(FlowButton button, bool isDown)
    {

    }
}
