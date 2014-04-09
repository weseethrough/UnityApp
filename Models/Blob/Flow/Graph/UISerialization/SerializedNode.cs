using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor; 
#endif

/// <summary>
/// single node of serialization tree structure. It manages local components using serializable settings and transform
/// </summary>
[System.Serializable]
public class SerializedNode : ISerializable 
{
    public List<SerializedNode> subBranches;
    private SerializableTransform transform;        
    private SerializableSettings settings;
    private string name;
    private string prefabName;

    /// <summary>
    /// default construction initializator
    /// </summary>    
    public SerializedNode()
    {
        this.subBranches = new List<SerializedNode>();
        this.transform = null;
        this.settings = null;
        this.name = string.Empty;
        this.prefabName = string.Empty;
    }
	
	/// <summary>
	/// constructor which builds node based on the provided game object and do required processing of its components trying to serialize them as well
	/// </summary>
	/// <param name="source">target gameobejct to eb prepared for serialziation</param>	
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

    /// <summary>
    /// deserialization constructor
    /// </summary>
    /// <param name="info">serialziation infor which stores all configuration data</param>
    /// <param name="ctxt">serialziation context</param>    
    public SerializedNode(SerializationInfo info, StreamingContext ctxt)
	{
        this.subBranches    = (List<SerializedNode>)info.GetValue("SubBranches", typeof(List<SerializedNode>));
        this.transform      = (SerializableTransform)info.GetValue("Transform", typeof(SerializableTransform));
        this.settings       = (SerializableSettings)info.GetValue("Settings", typeof(SerializableSettings));
        this.name           = (string)info.GetValue("Name", typeof(string));
        this.prefabName     = (string)info.GetValue("PrefabName", typeof(string));	        
	}
	
	/// <summary>
	/// serialziation function caleld by serializer
	/// </summary>
    /// <param name="info">serialziation infor which stores all configuration data</param>
    /// <param name="ctxt">serialziation context</param>
	/// <returns></returns>
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        info.AddValue("SubBranches", this.subBranches);
        info.AddValue("Transform", this.transform);
        info.AddValue("Settings", this.settings);
        info.AddValue("Name", this.name);
        info.AddValue("PrefabName", this.prefabName);
   	}
	
    /// <summary>
    /// update transform node from provided transform
    /// </summary>
    /// <param name="source">transform soruce to get prepared for serialization</param>
    /// <returns></returns>
    public void UpdateFromObject(Transform source)
    {
        this.transform = (source == null) ? null : new SerializableTransform(source);       
    }

    /// <summary>
    /// returns all data stored in serialization-ready form to provided transform
    /// </summary>
    /// <param name="target">trasfrom which is aimed to get updated from stored data</param>
    /// <returns></returns>
    public void WriteToTransform(Transform target)
    {
        if (this.transform != null)
        {
            transform.WriteToTransform(target);
        }
    }

    /// <summary>
    /// reads resource path of the provided source path stripping form unrequired parts (we do not need during runtime anything outside resource folder)
    /// </summary>
    /// <param name="source">source path sctring</param>
    /// <returns></returns>
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

    /// <summary>
    /// function which helps to rebuild gameobject structure as a child of the provided transform
    /// </summary>
    /// <param name="parentTranform">parent transform to which newly created reconstructed structure would be attachet to</param>
    /// <param name="allowDuplicates">should we allow to have two types with the same name or rather ignore if one exists already</param>
    /// <returns>rebuild class instance or recovered by name if no duplicates are available</returns>
    public Transform RebuildNode(Transform parentTranform, bool allowDuplicates)
    {
        return RebuildNode(parentTranform, allowDuplicates, null);
    }

    /// <summary>
    /// function which helps to rebuild gameobject structure as a child of the provided transform
    /// </summary>
    /// <param name="parentTranform">parent transform to which newly created reconstructed structure would be attachet to</param>
    /// <param name="allowDuplicates">should we allow to have two types with the same name or rather ignore if one exists already</param>
    /// <param name="overrideCollection">data to override collection, used for flow based custom settings</param>
    /// <returns>rebuild class instance or recovered by name if no duplicates are available</returns>
    public Transform RebuildNode(Transform parentTranform, bool allowDuplicates, FlowPanelComponent overrideCollection)
    {
        Transform t;
        //we will try to rebuild object form prefab source and then name and position it according to original notes
        if (prefabName != string.Empty)
        {
            t = parentTranform.FindChild(name);
            if (allowDuplicates || t == null)
            {

                GameObject prefab = Resources.Load(prefabName) as GameObject;
#if UNITY_EDITOR
                //this method allows us to keep prefab reference for saving purposes. During runtime it 
                //doesn't matter if we use prefabs or clan gameobjects but if while in editor we will try 
                //to save without this reference we will loose connection with source
                GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
#else
                GameObject go = GameObject.Instantiate(prefab) as GameObject;
            
#endif
                if (overrideCollection != null && overrideCollection.names != null)
                {
                    int id = overrideCollection.names.IndexOf(name);
                    if (id > -1)
                    {
                        //we have flow edited setting for this element. In this case we override settings before applying them to object
                        settings = overrideCollection.settings[id];
                    }

                }
                go.transform.parent = parentTranform;
                WriteToTransform(go.transform);
                settings.LoadSettingsTo(go);
                go.name = name;

                return go.transform;
            }
        }

        //prefab source have been not found or duplicate policy is violated, we will return child with searched name.
        //Note! if multiple children shares same name it might bring some confusion and rebuild errors!
        //TODO: find nice and simple way to identify child without name. 

        t = parentTranform.FindChild(name);
        return t;
    }

    /// <summary>
    /// gets prefab name/path used for resource identification. Eg prefab is named BasicButton while instance is named Button1 we will return BasicButton name (+ path if required for in resoruce identification)
    /// </summary>
    /// <returns>prefab name</returns>
    public string GetPrefabName()
    {
        return prefabName;
    }

    /// <summary>
    /// gets instance name used for in scene identification. Eg prefab is named BasicButton while instance is named Button1 we will return Button1
    /// </summary>
    /// <returns>instance name</returns>
    public string GetName()
    {
        return name;
    }

    /// <summary>
    /// finds associated serializable setting data
    /// </summary>
    /// <returns>serializable setting containing configuration of all component linked to the target gameobject</returns>
    public SerializableSettings GetSerializableSettings()
    {
        return settings;
    }
}
