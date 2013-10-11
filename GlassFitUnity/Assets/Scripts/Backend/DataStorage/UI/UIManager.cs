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

    public void LoadScene(SerializedNode source)
    {        
        RebuildStructure(gameObject.transform, source);             
    }
	
	SerializedNode ProcessBranch(GameObject go)
    {
        SerializedNode structureParent = new SerializedNode(go);
		Debug.Log(go.name);
		
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

    void RebuildStructure(Transform parent, SerializedNode node)
    {
		if (parent == null || node == null) return;
		
        Transform t = node.RebuildNode(parent.transform);
        for (int i = 0; i < node.subBranches.Count; i++)
        {
			if (t != null && node.subBranches[i] != null)
			{
            	RebuildStructure(t, node.subBranches[i]);
			}
        }
    }

}
