using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Collections.Generic;

/// <summary>
/// this class is used instead of dictionary which is not serializable. It allows to have undefined set of serializable objects identified by name
/// Designed to store ISerialziable elements
/// </summary>
[System.Serializable]
public class StorageDictionary : ISerializable 
{
    private List<ISerializable> data;
	private List<string> name;
	
	/// <summary>
	/// default contsrtuctor and initializtor
	/// </summary>
	/// <returns></returns>
	public StorageDictionary()
	{
		this.data = new List<ISerializable>();
		this.name = new List<string>();
	}
	
	/// <summary>
	/// deserialziation constructor
	/// </summary>
	/// <param name="info">serialziation infro containing class data</param>
	/// <param name="ctxt">serialziation context</param>
	/// <returns></returns>
	public StorageDictionary(SerializationInfo info, StreamingContext ctxt)
	{
		this.data = (List<ISerializable>)info.GetValue("Data", typeof(List<ISerializable>));
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
	/// <param name="obj">ISerialziation object pointer</param>
    /// <returns>true if successfully add</returns>
	public bool Add(string name, ISerializable obj)
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
	/// <param name="obj">ISerialziation pointer to be included in dictionary</param>
	/// <param name="name">object identification name</param>
	/// <returns>true if successfully add or changed</returns>
	public bool Set(ISerializable obj, string name)
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
    /// changes name identifier of the element in the dictionary
    /// </summary>
    /// <param name="name">name identifier of the object to change</param>
    /// <param name="newName">new name under whch object would exist from now on</param>
    /// <returns>true if succesfuly changed</returns>
    public bool Rename(string name, string newName)
    {
        if (this.data.Count != this.name.Count)
        {
            Debug.LogError("StorageDictionary out of sync!");
            return false;
        }

        int index = this.name.FindIndex(x => x == name);
        if (index >= 0)
        {
            this.name[index] = newName;            

            return true;
        }

        return false;
    }
		
	/// <summary>
	/// removes object from dictionary along with its key identifier
	/// </summary>
	/// <param name="obj">object to be removed</param>
    /// <returns>true if removal successfull</returns>
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
	
	/// <summary>
	/// gets element under specified name identifier
	/// </summary>
	/// <param name="name">string name identifier of the element</param>
	/// <returns>element associated with name identifier</returns>
	public ISerializable Get(string name)
	{
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync!");
			return null;
		}
		
		int index = this.name.FindIndex(x => x == name);
        if (index >= 0)
        {
            return data[index];
        }
        else
        {
            Debug.Log("-B------------------START-------------------B-");
            Debug.Log("StorageDictionary does not contain " + name);
            Debug.Log("item count: " + data.Count);
            Debug.Log("-B-------------------END--------------------B-");
        }
        return null;
	}

    /// <summary>
    /// gets element under specified index, used for iteration processes
    /// </summary>
    /// <param name="index">index of the interest</param>
    /// <returns>object under specified index</returns>
    public ISerializable Get(int index)
    {
        if (this.data.Count != this.name.Count)
        {
            Debug.LogError("StorageDictionary out of sync!");
            return null;
        }
        
        if (index >= 0 && index < data.Count)
        {
            return data[index];
        }
        return null;
    }

    /// <summary>
    /// gets name and data under specified index
    /// </summary>
    /// <param name="index">index of the object of the interest</param>
    /// <param name="name">name identifier under specified index</param>
    /// <param name="data">data under specified index</param>
    /// <returns></returns>
    public void Get(int index, out string name, out ISerializable data)
    {
        if (this.data.Count != this.name.Count)
        {
            Debug.LogError("StorageDictionary out of sync!");
            name = string.Empty;
            data = null;
            return;
        }

        if (index >= 0 && index < this.data.Count)
        {
            name = this.name[index];
            data = this.data[index];
            return;
        }

        name = string.Empty;
        data = null;

    }

    /// <summary>
    /// gets index of specified name identifier
    /// </summary>
    /// <param name="name">name identifier</param>
    /// <returns>index under which name identifier exists or -1 if it doesnt</returns>
    public int GetIndex(string name)
    {
        if (this.data.Count != this.name.Count)
        {
            Debug.LogError("StorageDictionary out of sync!");            
            return -1;
        }

        int index = this.name.FindIndex(x => x == name);
        return index;
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
	/// checks if specified object exist in dictionary
	/// </summary>
	/// <param name="obj">object to look for</param>
	/// <returns>true if object exists in the dictionary</returns>
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
