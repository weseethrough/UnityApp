using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Collections.Generic;


//this class is used instead of dictionary which is not serializable. It allows to have undefined set of serialziable objects identified by name

[System.Serializable]
public class StorageDictionary : ISerializable 
{
    private List<ISerializable> data;
	private List<string> name;
	
	public StorageDictionary()
	{
		this.data = new List<ISerializable>();
		this.name = new List<string>();
	}
	
	public StorageDictionary(SerializationInfo info, StreamingContext ctxt)
	{
		this.data = (List<ISerializable>)info.GetValue("Data", typeof(List<ISerializable>));
		this.name = (List<string>)info.GetValue("Name", typeof(List<string>));
		
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync at deserialization stage!");			
		}
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
    	info.AddValue("Data", this.data);
		info.AddValue("Name", this.name);
   	}
	
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
        if (index >= 0)
        {
            return data[index];
        }
        return null;
	}

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

    public int Length()
    {
        if (this.data.Count != this.name.Count)
        {
            Debug.LogError("StorageDictionary out of sync!");
            return 0;
        }

        return this.data.Count;
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
