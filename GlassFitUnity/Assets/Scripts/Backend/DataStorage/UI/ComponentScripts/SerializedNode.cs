using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor; 
#endif

[System.Serializable]
public class SerializedNode : ISerializable 
{
    public List<ISerializable> subBranches;
    private SerializableTransform transform;        
    private SerializableSettings settings;
    private string name;
    private string prefabName;

    public SerializedNode()
    {
        this.subBranches = new List<ISerializable>();
        this.transform = null;
        this.settings = null;
        this.name = string.Empty;
        this.prefabName = string.Empty;
    }
	
	public SerializedNode(GameObject source)
	{
        string path = string.Empty;
        
#if UNITY_EDITOR
        path = AssetDatabase.GetAssetPath(source);
#endif

        string resourcePath = GetResourcePath(path);

        if (resourcePath.Length > 0)
        {
            this.subBranches = new List<ISerializable>();
            this.transform = new SerializableTransform(source.transform);            
            this.settings = new SerializableSettings(source);
            this.prefabName = resourcePath;
        }
        else
        {
            this.subBranches = new List<ISerializable>();
            this.settings = null;
            this.transform = null;            
            this.prefabName = string.Empty;
        }

        this.name = source != null ? source.name : string.Empty;
	}

    public SerializedNode(SerializationInfo info, StreamingContext ctxt)
	{
        this.subBranches    = (List<ISerializable>)info.GetValue("SubBranches", typeof(List<ISerializable>));
        this.transform      = (SerializableTransform)info.GetValue("Transform", typeof(SerializableTransform));        
        this.name           = (string)info.GetValue("Name", typeof(string));
        this.prefabName     = (string)info.GetValue("PrefabName", typeof(string));	        
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        info.AddValue("SubBranches", this.subBranches);
        info.AddValue("Transform", this.transform);

        info.AddValue("Name", this.name);
        info.AddValue("PrefabName", this.prefabName);
   	}
	
    public void UpdateFromObject(Transform source)
    {
        this.transform = (source == null) ? null : new SerializableTransform(source);
    }

    public void WriteToTransform(Transform target)
    {
        if (this.transform != null)
        {
            transform.WriteToTransform(target);
        }
    }

    public string GetResourcePath(string source)
    {
        string resourcesFolder = "Resources/";
        int index = source.LastIndexOf(resourcesFolder);
        if (index == -1 && source.Length > 0)
        {
            Debug.LogError("All assets to be serialized need to be prefabs saved in Resources folder!");
            return string.Empty;
        }

        return source.Substring(index + resourcesFolder.Length);
    }
}
