using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

[Serializable()]
public class SimpleLocalizationTable : ISerializable 
{
	public StorageDictionary localizations;
	
	public SimpleLocalizationTable()
	{
		localizations = new StorageDictionary();	
	}
    
	public SimpleLocalizationTable(SerializationInfo info, StreamingContext ctxt)
	{
		this.localizations = (StorageDictionary)info.GetValue("Data", typeof(StorageDictionary));
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
    	info.AddValue("Data", this.localizations);
   	}
}
