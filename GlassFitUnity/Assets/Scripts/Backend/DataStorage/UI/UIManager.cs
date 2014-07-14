using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Base component for UI management, saving and restoring whole scenes
/// </summary>
public class UIManager : MonoBehaviour 
{
    static public string UIPannels = "UIPannels";
		
	/// <summary>
	/// default unity initialziation function which prepares this class to not be destroyed upon leaving scene
	/// </summary>
	/// <returns></returns>
	void Awake() {
		DontDestroyOnLoad(transform.gameObject);
	}
	
	/// <summary>
	/// serializes currently open ui scene
	/// </summary>
	/// <returns>root serialzization node</returns>
	public SerializedNode SaveScene()
	{
        SerializedNode node = null;
        if (gameObject.transform.childCount > 0)
        {
            GameObject go = gameObject.transform.GetChild(0).gameObject;
            node = ProcessBranch(go);
        }        
        return node;
	}

    /// <summary>
    /// builds ui scene structure from serialized node
    /// </summary>
    /// <param name="source">serialized root source to get into reconstruction pipeline</param>
    /// <returns>result cloned widget root object</returns>
    public GameObject LoadScene(SerializedNode source)
    {
        return RebuildStructure(gameObject.transform, source, "", null);
    }


    /// <summary>
    /// builds ui scene structure from serialized node
    /// </summary>
    /// <param name="source">serialized root source to get into reconstruction pipeline</param>    
    /// <param name="cloneInstanceName">name of the widget root for be cloned</param>
    /// <returns>result cloned widget root object</returns>
    public GameObject LoadScene(SerializedNode source, string cloneInstanceName)
    {
        return RebuildStructure(gameObject.transform, source, cloneInstanceName, null);             
    }

    /// <summary>
    /// builds ui scene structure from serialized node
    /// </summary>
    /// <param name="source">serialized root source to get into reconstruction pipeline</param>    
    /// <param name="cloneInstanceName">name of the widget root for be cloned</param>    
    /// <param name="overrideCollection">collection of data to override elements within reconstruction process into some custom settings</param>
    /// <returns>result cloned widget root object</returns>
    public GameObject LoadScene(SerializedNode source, string cloneInstanceName, FlowPanelComponent overrideCollection)
    {
        return RebuildStructure(gameObject.transform, source, cloneInstanceName, overrideCollection);             
    }
	    
	/// <summary>
	/// Processes gameobject and its children into serializable node
	/// </summary>
	/// <param name="go">root point to start processing from</param>
	/// <returns>tree of children processed into serializable nodes</returns>
	SerializedNode ProcessBranch(GameObject go)
    {
        SerializedNode structureParent = new SerializedNode(go);
		
        foreach (Transform child in go.transform)
        {
            //check if child subbranch should be processed
            UISerializable script = child.gameObject.GetComponent<UISerializable>();
            
            //if script is null then we will check if any child have this script.
            script = script != null? script : child.gameObject.GetComponentInChildren<UISerializable>();            
            if (script)
            {
                structureParent.subBranches.Add(ProcessBranch(child.gameObject));
            }            
        }

        return structureParent;
    }

    /// <summary>
    /// rebuilds structure using serializable nodes as a child of "parent", allows to clone instance of the the selected gameobject creating widget root
    /// </summary>
    /// <param name="parent">paretn transform to attach evertyhing to</param>
    /// <param name="node">serialziable node data</param>
    /// <param name="cloneInstanceName">name of the widget root to be cloned, can be empty</param>
    /// <returns>cloned widget root, can be null </returns>
    GameObject RebuildStructure(Transform parent, SerializedNode node, string cloneInstanceName)
    {
        return RebuildStructure(parent, node, cloneInstanceName, null);
    }

    /// <summary>
    /// rebuilds structure using serializable nodes as a child of "parent", allows to clone instance of the the selected gameobject creating widget root
    /// </summary>
    /// <param name="parent">paretn transform to attach evertyhing to</param>
    /// <param name="node">serialziable node data</param>
    /// <param name="cloneInstanceName">name of the widget root to be cloned, can be empty</param>
    /// <param name="overrideCollection">collection of the data to override setting and values in reconstructed trees' components</param>
    /// <returns>cloned widget root, can be null </returns>
    GameObject RebuildStructure(Transform parent, SerializedNode node, string cloneInstanceName, FlowPanelComponent overrideCollection)
    {
		if (parent == null || node == null) return null;

        GameObject searchedInstance = null;
        Transform t = node.RebuildNode(parent.transform, cloneInstanceName == "", overrideCollection);
        if (t != null && t.name == cloneInstanceName)
        {
            //get copy instead
            searchedInstance = (GameObject)GameObject.Instantiate(t.gameObject);
            searchedInstance.transform.parent = t.parent;
            searchedInstance.transform.localScale = t.localScale;
            t = searchedInstance.transform;
            cloneInstanceName = "";
        }

        for (int i = 0; i < node.subBranches.Count; i++)
        {
			if (t != null && node.subBranches[i] != null)
			{
                GameObject go = RebuildStructure(t, node.subBranches[i], cloneInstanceName, overrideCollection);
                if (searchedInstance == null)
                {
                    searchedInstance = go;
                }
			}
        }
        return searchedInstance;
    }
}
