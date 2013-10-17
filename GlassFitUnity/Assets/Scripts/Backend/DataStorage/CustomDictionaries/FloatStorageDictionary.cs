using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

//this class is used instead of dictionary which is not serializable. It allows to have undefined set of serialziable objects identified by name

[System.Serializable]
public class FloatStorageDictionary : ISerializable 
{
    private List<float> data;
	private List<string> name;
	
	public FloatStorageDictionary()
	{
        this.data = new List<float>();
		this.name = new List<string>();
	}

    public FloatStorageDictionary(SerializationInfo info, StreamingContext ctxt)
	{
        this.data = (List<float>)info.GetValue("Data", typeof(List<float>));
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

    public bool Add(string name, float obj)
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

    public bool Set(float obj, string name)
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

    public void Get(int index, out string name, out float data)
	{
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync!");
			name = string.Empty;
			data = 0;
			return;
		}
		
		if (index >=0 && index < this.data.Count)
		{
			name = this.name[index];
			data = this.data[index];			
			return;			
		}		
		
		name = string.Empty;
		data = 0;
		
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

    public float Get(string name)
	{
		if (this.data.Count != this.name.Count)
		{
			Debug.LogError("StorageDictionary out of sync!");
			return 0;
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
	
	
}
