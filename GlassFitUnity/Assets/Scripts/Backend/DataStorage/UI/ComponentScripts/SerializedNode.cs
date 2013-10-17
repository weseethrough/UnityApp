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
    public List<SerializedNode> subBranches;
    private SerializableTransform transform;        
    private SerializableSettings settings;
    private string name;
    private string prefabName;

    public SerializedNode()
    {
        this.subBranches = new List<SerializedNode>();
        this.transform = null;
        this.settings = null;
        this.name = string.Empty;
        this.prefabName = string.Empty;
    }
	
	public SerializedNode(GameObject source)
	{
        string path = string.Empty;
        
#if UNITY_EDITOR
        UnityEngine.Object parentObject = PrefabUtility.GetPrefabParent(source);        
        if (parentObject != null)
        {
            path = AssetDatabase.GetAssetPath(parentObject);
        }
        
#endif
        UISerializable script = source.GetComponent<UISerializable>();
        string resourcePath = string.Empty;
        if (script != null)
        {
            resourcePath = GetResourcePath(path);
        }
        
        if (resourcePath.Length > 0)
        {        
            this.transform = new SerializableTransform(source.transform);            
            this.settings = new SerializableSettings(source);
            this.prefabName = resourcePath;
        }
        else
        {            
            this.settings = null;
            this.transform = null;            
            this.prefabName = string.Empty;
        }

        this.subBranches = new List<SerializedNode>();
        this.name = source != null ? source.name : string.Empty;
	}

    public SerializedNode(SerializationInfo info, StreamingContext ctxt)
	{
        this.subBranches    = (List<SerializedNode>)info.GetValue("SubBranches", typeof(List<SerializedNode>));
        this.transform      = (SerializableTransform)info.GetValue("Transform", typeof(SerializableTransform));
        this.settings       = (SerializableSettings)info.GetValue("Settings", typeof(SerializableSettings));
        this.name           = (string)info.GetValue("Name", typeof(string));
        this.prefabName     = (string)info.GetValue("PrefabName", typeof(string));	        
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        info.AddValue("SubBranches", this.subBranches);
        info.AddValue("Transform", this.transform);
        info.AddValue("Settings", this.settings);
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
        string dot = ".";
        int index = source.LastIndexOf(resourcesFolder);
        int dotIndex = source.LastIndexOf(dot);

        if (index == -1 && source.Length > 0)
        {
            Debug.LogError("All assets to be serialized need to be prefabs saved in Resources folder!");
            return string.Empty;
        }
		else if (index == -1) 
		{
			return string.Empty;
		}
        int starting = index + resourcesFolder.Length;
        source = source.Substring(index + resourcesFolder.Length, dotIndex - starting);

        return source;
    }

    public Transform RebuildNode(Transform parentTranform)
    {
        //we will try to rebuild object form prefab source and then name and position it according to original notes
        if (prefabName != string.Empty)
        {
            GameObject prefab = Resources.Load(prefabName) as GameObject;
#if UNITY_EDITOR
            GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
#else
            GameObject go = GameObject.Instantiate(prefab) as GameObject;
            
#endif
            go.transform.parent = parentTranform;
            WriteToTransform(go.transform);
            settings.LoadSettingsTo(go);
            go.name = name;

            return go.transform;
        }

        //object have been not found, we will return child with searched name.
        //Note! if multiple children shares same name it might bring some confusion and rebuild errors!
        //TODO: find nice and simple way to identify child without name. (index? is it trusted way to go?)

        Transform t = parentTranform.FindChild(name);
        return t;
    }
}
