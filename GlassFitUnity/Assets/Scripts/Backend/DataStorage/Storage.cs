﻿using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

[Serializable()]
public class Storage : ISerializable 
{
	public StorageDictionary dictionary;
	
	public Storage()
	{
		dictionary = new StorageDictionary();	
	}
    
	public Storage(SerializationInfo info, StreamingContext ctxt)
	{
		this.dictionary = (StorageDictionary)info.GetValue("Data", typeof(StorageDictionary));
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
    	info.AddValue("Data", this.dictionary);
   	}
}