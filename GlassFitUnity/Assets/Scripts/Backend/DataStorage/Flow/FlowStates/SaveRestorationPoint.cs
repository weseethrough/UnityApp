using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

/// <summary>
/// Point which tries to restore latest save
/// </summary>
[Serializable]
public class SaveRestorationPoint : FlowState 
{
	/// <summary>
	/// default constructor
	/// </summary>	
	public SaveRestorationPoint() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public SaveRestorationPoint(SerializationInfo info, StreamingContext ctxt)
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

        return "Save Restoration Point";
    }

    /// <summary>
    /// initialzies node and creates name for it. Makes as well default iput/output connection sockets
    /// </summary>
    /// <returns></returns>
    protected override void Initialize()
    {
        base.Initialize();

        Size = new Vector2(175, 80);        
        NewInput("In", "Flow");
        NewOutput("Out", "Flow");
        NewParameter("Name", GraphValueType.String, "Save Restoration Point");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override void Entered()
    {
        base.Entered();
        DataStore.LoadStorage(DataStore.BlobNames.saves);

        Storage storage = DataStore.GetStorage(DataStore.BlobNames.saves);
        
        GameStateRestorable gsRestorable = storage != null ? storage.dictionary.Get("Save1") as GameStateRestorable : null;
        if (gsRestorable != null && gsRestorable.GoToRestoredState())
        {
            
        }
        else if (Outputs.Count > 0 && parentMachine != null)
        {
            parentMachine.FollowConnection(Outputs[0]);
        }
        else
        {
            Debug.LogError("Dead end start");
        }
    }

    /// <summary>
    /// State is exiting, in which case we are no longer interseted in having this restoration point
    /// </summary>
    /// <returns></returns>
    public override void Exited()
    {
        base.Exited();
        Storage storage = DataStore.GetStorage(DataStore.BlobNames.saves);
        if (storage.dictionary.Contains("Save1"))
        {
            storage.dictionary.Remove("Save1");
            DataStore.SaveStorage(DataStore.BlobNames.saves);
        }
    }

    public override bool CallStaticFunction(string functionName, FlowButton caller)
    {
        MemberInfo[] info = typeof(ButtonFunctionCollection).GetMember(functionName);

        if (info.Length == 1)
        {
            System.Object[] newParams = new System.Object[2];
            newParams[0] = caller;
            newParams[1] = this;
            bool ret = (bool)typeof(ButtonFunctionCollection).InvokeMember(functionName,
                                    BindingFlags.InvokeMethod |
                                    BindingFlags.Public |
                                    BindingFlags.Static,
                                    null, null, newParams);
            return ret;
        }

        return true;
    }
}
