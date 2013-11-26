using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System;

/// <summary>
/// this class is used instead of dictionary which is not serializable. It allows to have undefined set of serializable objects identified by name
/// Designed to store basic types (int, double, float, bool, string...)
/// </summary>
[Serializable]
public class StorageDictionaryBase<T> : ISerializable 
{
    protected List<T> data;
	protected List<string> name;
	
	/// <summary>
	/// default constructor and initializator
	/// </summary>	
	public StorageDictionaryBase()
	{
		this.data = new List<T>();
		this.name = new List<string>();
	}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">serialziation infro containing class data</param>
    /// <param name="ctxt">serialziation context</param>
    /// <returns></returns>
    public StorageDictionaryBase(SerializationInfo info, StreamingContext ctxt)
	{
		this.data = (List<T>)info.GetValue("Data", typeof(List<T>));
		this.name = (List<string>)info.GetValue("Name", typeof(List<string>));
		
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync at deserialization stage!");		
		}
	}

    /// <summary>
    /// serialization function called by serializer
    /// </summary>
    /// <param name="info">serialzieation infor gatherring class details</param>
    /// <param name="ctxt">serialziation context</param>
    /// <returns></returns>
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
    	info.AddValue("Data", this.data);
		info.AddValue("Name", this.name);
   	}

    /// <summary>
    /// adds new element to the dictionary
    /// </summary>
    /// <param name="name">name identifier</param>
    /// <param name="obj">variable of type T to be add to the dictionary</param>
    /// <returns>true if successfully add</returns>
	public bool Add(string name, T obj)
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

    /// <summary>
    /// changes or creates new key in dictionary
    /// </summary>
    /// <param name="obj">variable of type T to be included in dictionary</param>
    /// <param name="name">object identification name</param>
    /// <returns>true if successfully add or changed</returns>
	public bool Set(T obj, string name)
	{
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync!");
			return false;
		}
		
		int index = this.name.FindIndex(x => x == name);
		if (index >= 0)
		{
			this.data[index] = obj;
			return true;
		}
		
		this.name.Add(name);
		this.data.Add(obj);	
		return true;
	}

    /// <summary>
    /// removes element from dictionary along with its identifier
    /// </summary>
    /// <param name="name">element name identifier</param>
    /// <returns>true if successfully removed</returns>
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

    /// <summary>
    /// gets element under specified name identifier
    /// </summary>
    /// <param name="name">string name identifier of the element</param>
    /// <returns>element associated with name identifier</returns>
    public T Get(int index)
    {
        if (this.data.Count != this.name.Count)
        {
            Debug.LogError("StorageDictionary out of sync!");
            return default(T);
        }

        if (index >= 0 && index < data.Count)
        {
            return data[index];
        }
        return default(T);
    }

    /// <summary>
    /// gets name and data under specified index
    /// </summary>
    /// <param name="index">index of the object of the interest</param>
    /// <param name="name">name identifier under specified index</param>
    /// <param name="data">data under specified index</param>
    /// <returns></returns>
	public void Get(int index, out string name, out T data)
	{
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync!");
			name = string.Empty;
            data = default(T);
			return;
		}
		
		if (index >=0 && index < this.data.Count)
		{
			name = this.name[index];
			data = this.data[index];			
			return;			
		}		
		
		name = string.Empty;
        data = default(T);
		
	}

    /// <summary>
    /// checks how many elements dictionary contains
    /// </summary>
    /// <returns>number of the elements in the dictionary</returns>
	public int Length()
	{
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync!");
			return 0;
		}
		
		return this.data.Count;
	}

    /// <summary>
    /// gets element under specified name identifier
    /// </summary>
    /// <param name="index">name identifier of the element of the interest</param>
    /// <returns>variable data under specified name</returns>
	public T Get(string name)
	{
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync!");
            return default(T);
		}
		
		int index = this.name.FindIndex(x => x == name);
        if (index < 0)
        {
            return default(T);
        }
		return data[index];
	}

    /// <summary>
    /// checks if specified name identifier exists in the dictionary
    /// </summary>
    /// <param name="name">name identifier to look for</param>
    /// <returns>true if identifier found</returns>
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


    /// <summary>
    /// helps to clone whole dictionary
    /// </summary>
    /// <returns>returns copy of this dictionary</returns>
    public StorageDictionaryBase<T> Clone()
    {
        StorageDictionaryBase<T> clone = new StorageDictionaryBase<T>();
        clone.data.AddRange(this.data);
        clone.name.AddRange(this.name);

        return clone;
    }
}
