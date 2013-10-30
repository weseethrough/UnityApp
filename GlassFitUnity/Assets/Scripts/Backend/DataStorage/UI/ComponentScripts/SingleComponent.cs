using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor; 
using System.Reflection;
#endif


[System.Serializable]
public class SingleComponent : ISerializable 
{
    public StorageDictionaryBase<int>       intData;
    public StorageDictionaryBase<double>    doubleData;
    public StorageDictionaryBase<string>    strData;
    public string                           name;
    
    public SingleComponent()
    {       
        this.intData    = new StorageDictionaryBase<int>();
        this.doubleData = new StorageDictionaryBase<double>();
        this.strData    = new StorageDictionaryBase<string>();
        name            = string.Empty;
    }    

    public SingleComponent(SerializationInfo info, StreamingContext ctxt)
	{
        this.intData        = (StorageDictionaryBase<int>)info.GetValue("IntData", typeof(StorageDictionaryBase<int>));
        this.doubleData     = (StorageDictionaryBase<double>)info.GetValue("FloatData", typeof(StorageDictionaryBase<double>));
        this.strData        = (StorageDictionaryBase<string>)info.GetValue("StrData", typeof(StorageDictionaryBase<string>));
        this.name           = (string)info.GetValue("Name", typeof(string));                
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        info.AddValue("IntData", this.intData);
        info.AddValue("FloatData", this.doubleData);
        info.AddValue("StrData", this.strData);
        info.AddValue("Name", this.name);
   	}

    public StorageDictionaryBase<int> GetInitializedIntDict()
    {
        if (intData == null) intData = new StorageDictionaryBase<int>();
        return intData;
    }

    public StorageDictionaryBase<double> GetInitializedFloatDict()
    {
        if (doubleData == null) doubleData = new StorageDictionaryBase<double>();
        return doubleData;
    }

    public StorageDictionaryBase<string> GetInitializedStrDict()
    {
        if (strData == null) strData = new StorageDictionaryBase<string>();
        return strData;
    }

    public SingleComponent Clone()
    {
        SingleComponent sc = new SingleComponent();
        if (intData != null)
        {
            sc.intData = intData.Clone();
        }
        if (doubleData != null)
        {
            sc.doubleData = doubleData.Clone();
        }
        if (strData != null)
        {
            sc.strData = strData.Clone();
        }

        sc.name = name;
        return sc;
    }
}
