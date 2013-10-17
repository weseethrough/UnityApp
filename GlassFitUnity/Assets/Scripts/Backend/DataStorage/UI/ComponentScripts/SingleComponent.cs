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
    public IntStorageDictionary         intData;
    public FloatStorageDictionary       floatData;
    public StringStorageDictionary      strData;
    public string                       name;
    
    public SingleComponent()
    {       
        this.intData    = new IntStorageDictionary();
        this.floatData  = new FloatStorageDictionary();
        this.strData    = new StringStorageDictionary();
        name            = string.Empty;
    }    

    public SingleComponent(SerializationInfo info, StreamingContext ctxt)
	{
        this.intData                = (IntStorageDictionary)info.GetValue("IntData", typeof(IntStorageDictionary));
        this.floatData              = (FloatStorageDictionary)info.GetValue("FloatData", typeof(FloatStorageDictionary));
        this.strData                = (StringStorageDictionary)info.GetValue("StrData", typeof(StringStorageDictionary));
        this.name                   = (string)info.GetValue("Name", typeof(string));                
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        info.AddValue("IntData", this.intData);
        info.AddValue("FloatData", this.floatData);
        info.AddValue("StrData", this.strData);
        info.AddValue("Name", this.name);
   	}

    public IntStorageDictionary GetInitializedIntDict()
    {
        if (intData == null) intData = new IntStorageDictionary();
        return intData;
    }

    public FloatStorageDictionary GetInitializedFloatDict()
    {
        if (floatData == null) floatData = new FloatStorageDictionary();
        return floatData;
    }

    public StringStorageDictionary GetInitializedStrDict()
    {
        if (strData == null) strData = new StringStorageDictionary();
        return strData;
    }	
}
