using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// exit state which is simply blank end. It doesn't have any navigation out connections.
/// </summary>
[Serializable]
public class SaveRestorableArea : FlowState 
{
    static public bool allowGameStateAutoSave = false;

	/// <summary>
	/// default constructor
	/// </summary>	
	public SaveRestorableArea() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public SaveRestorableArea(SerializationInfo info, StreamingContext ctxt)
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

        return "Save Restorable Area";
    }

    /// <summary>
    /// initialzies node and creates name for it. Makes as well default iput/output connection sockets
    /// </summary>
    /// <returns></returns>
    protected override void Initialize()
    {
        base.Initialize();

        Size = new Vector2(175, 80);        
        
        NewParameter("Name", GraphValueType.String, "Save Restorable Area");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override void Entered()
    {
        base.Entered();
        allowGameStateAutoSave = true;
        
    }

    /// <summary>
    /// State is exiting, in which case we are no longer interseted in having this restoration point
    /// </summary>
    /// <returns></returns>
    public override void Exited()
    {
        Storage storage = DataStore.GetStorage(DataStore.BlobNames.saves);
        storage.dictionary.Remove("Save1");
        allowGameStateAutoSave = false;
    }
}
