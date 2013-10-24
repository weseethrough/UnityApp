using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[ExecuteInEditMode]
public class UIManager : MonoBehaviour 
{
    static public string UIPannels = "UIPannels";
		
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

    public GameObject LoadScene(SerializedNode source)
    {
        return RebuildStructure(gameObject.transform, source, "");
    }

    public GameObject LoadScene(SerializedNode source, string cloneInstanceName)
    {
        return RebuildStructure(gameObject.transform, source, cloneInstanceName);             
    }
	
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

    GameObject RebuildStructure(Transform parent, SerializedNode node, string cloneInstanceName)
    {
		if (parent == null || node == null) return null;

        GameObject searchedInstance = null;
        Transform t = node.RebuildNode(parent.transform, cloneInstanceName == "");
        if (t != null && t.name == cloneInstanceName)
        {
            //get copy instead
            searchedInstance = (GameObject)GameObject.Instantiate(t.gameObject);
            searchedInstance.transform.parent = t.parent;
            t = searchedInstance.transform;
            cloneInstanceName = "";
        }

        for (int i = 0; i < node.subBranches.Count; i++)
        {
			if (t != null && node.subBranches[i] != null)
			{
                GameObject go = RebuildStructure(t, node.subBranches[i], cloneInstanceName);
                if (searchedInstance == null)
                {
                    searchedInstance = go;
                }
			}
        }
        return searchedInstance;
    }
}
