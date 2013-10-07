﻿using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

//this class is used instead of dictionary which is not serializable. It allows to have undefined set of serialziable objects identified by name

[Serializable()]
public class StorageDictionary : ISerializable 
{
    private List<ISerializable> data;
	private List<String> name;
	
	public StorageDictionary()
	{
		this.data = new List<ISerializable>();
		this.name = new List<String>();
	}
	
	public StorageDictionary(SerializationInfo info, StreamingContext ctxt)
	{
		this.data = (List<ISerializable>)info.GetValue("Data", typeof(List<ISerializable>));
		this.name = (List<String>)info.GetValue("Name", typeof(List<String>));
		
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync at deserialization stage!");
			return null;
		}
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
    	info.AddValue("Data", this.data);
		info.AddValue("Name", this.name);
   	}
	
	public bool Add(ISerializable obj, string name)
	{
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync!");
			return false;
		}
		
		int index = this.name.FindIndex(x => x == name);
		if (index >= 0)
		{
			//we have this object in our list
			Debug.LogWarning("Object "+name+ " were trying to overwrite another object already existing in dictionary");
			return false;	
		}
		
		this.name.Add(name);
		this.data.Add(obj);	
		return true;
	}
		
	public bool Remove(string name)
	{
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync!");
			return false;
		}
		
		int index = this.name.FindIndex(x => x == name);
		if (index >= 0)
		{
			this.name.RemoveAt(index);
			this.data.RemoveAt(index);	
		
			return true;	
		}
		
		return false;
		
	}
		
	public bool Remove(ISerializable obj)
	{
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync!");
			return false;
		}
		
		int index = this.data.FindIndex(x => x == obj);
		if (index >= 0)
		{
			this.name.RemoveAt(index);
			this.data.RemoveAt(index);	
		
			return true;	
		}
		
		return false;	
	}
	
	public ISerializable Get(string name)
	{
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync!");
			return null;
		}
		
		int index = this.name.FindIndex(x => x == name);
		return data[index];
	}
	
	public bool Contains(string name)
	{
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync!");
			return false;
		}
		
		int index = this.name.FindIndex(x => x == name);
		return index >= 0;
	}
	
	public bool Contains(ISerializable obj)
	{
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync!");
			return false;
		}
		
		int index = this.data.FindIndex(x => x == obj);
		return index >= 0;
	}
	
}
