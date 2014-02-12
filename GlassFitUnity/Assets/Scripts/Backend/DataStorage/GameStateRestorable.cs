using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

/// <summary>
/// storage class is simply wrapping class around storage dictionary which allows to plug and unplug easily new classes without losing reference to this root point in places which uses it directly
/// </summary>
[System.Serializable]
public class GameStateRestorable : ISerializable 
{
    public uint? flowStateID = null;
    public string flowName;
    public StorageDictionaryBase<System.Object> variables = new StorageDictionaryBase<System.Object>();


	/// <summary>
	/// default constructor and initialization
	/// </summary>
	/// <returns></returns>
	public GameStateRestorable()
	{        
        
	}
    
	/// <summary>
	/// deserialization constructor
	/// </summary>
	/// <param name="info">serialziation info containing data about all subvariables</param>
	/// <param name="ctxt">serialziation context</param>	
    public GameStateRestorable(SerializationInfo info, StreamingContext ctxt)
	{
        foreach (SerializationEntry entry in info)
        {
            switch (entry.Name)
            {
                case "flowStateID":
                    this.flowStateID = entry.Value as uint?;
                    break;
                case "flowName":
                    this.flowName = entry.Value as string;
                    break;
                case "variables":
                    this.variables = entry.Value as StorageDictionaryBase<System.Object>;
                    break;
            }
        }
	}
	
	/// <summary>
	/// serialization function called by serializer
	/// </summary>
    /// <param name="info">serialziation info containing data about all subvariables</param>
    /// <param name="ctxt">serialziation context</param>
	/// <returns></returns>
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{

        info.AddValue("flowStateID", this.flowStateID);
        info.AddValue("flowName", this.flowName);
        info.AddValue("variables", this.variables);
   	}

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public void StoreCurrent(FlowStateMachine fsm)
    {
        uint id;
        
        //get state ID
        if (fsm.GetCurrentTargetState() == null)
        {
            FlowState fs = fsm.GetCurrentState();
            id = fs.Id;
        }
        else
        {
            id = fsm.GetCurrentTargetState().Id;
        }

        flowStateID = id;

        //Get flow name
        GraphComponent gc = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        int flowIndex = gc.GetSelectedFlowIndex();
        Storage flowStorage = DataStore.GetStorage(DataStore.BlobNames.flow);        
        string name;
        ISerializable flowObj;
        flowStorage.dictionary.Get(flowIndex, out name, out flowObj);

        flowName = name;

        //get vault data
        foreach (KeyValuePair<string, DataEntry> pair in DataVault.data)
        {
            DataEntry de = pair.Value;
            System.Object obj = de.storedValue;

            System.Type type = obj.GetType();
            if (type.IsPrimitive || type.Equals(typeof(string)) || obj is GameRestorableObject)
            {
                variables.Add(pair.Key, pair.Value);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public void RestoreCurrent()
    {        

        DataVault.data.Clear();

        for (int i = 0; i < variables.Length(); i++)
        {
            string name;
            System.Object de;
            variables.Get(i, out name, out de);

            DataVault.data[name] = de as DataEntry;
        }

        FlowStateMachine fsm = GameObject.FindObjectOfType(typeof(FlowStateMachine)) as FlowStateMachine;
        if (flowStateID != null)
        {            
            fsm.GoToStateByID((uint)flowStateID);
        }        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool GoToRestoredState()
    {
        GraphComponent gc = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;
        int nextIndex = flowDictionary.GetIndex(flowName);
        if (gc.GetSelectedFlowIndex() == nextIndex) 
        {
            RestoreCurrent();
            return true;
        }
        return false;
    }
}
