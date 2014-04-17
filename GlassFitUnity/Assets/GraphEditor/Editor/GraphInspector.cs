using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

/// (C) Copyright 2013 by Paul C. Isaac 
[CustomEditor(typeof(GraphComponent))]
public class GraphInspector : Editor
{
    private string newFlowName = "NewFlowName";

	public override void OnInspectorGUI ()
	{
		//GraphComponent c = target as GraphComponent;
        
        GraphComponent componentSource = target as GraphComponent;

        if (GUILayout.Button("Force load from drive"))
        {
            DataStore.LoadStorage(DataStore.BlobNames.flow);
            componentSource.SetSelectedFlowIndex(0);
            GraphWindow.Init();
        }

		EditorGUIUtility.LookLikeControls(144);      
		DrawDefaultInspector();        

        GraphComponent gc = target as GraphComponent;
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Available Flows");            
            int oldIndex = componentSource.GetSelectedFlowIndex();
            int flowIndex = EditorGUILayout.Popup(oldIndex, GetFlowList());

            if (oldIndex != flowIndex)
            {
                componentSource.SetSelectedFlowIndex(flowIndex);
                GraphWindow.Init();
            }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();        
            if (GUILayout.Button("Reload Current"))
            {            
                componentSource.SetSelectedFlowIndex();
                GraphWindow.Init();                
            }
            if (GUILayout.Button(new GUIContent("Save flows", "This should be done automatically with every change. If not then this is a safety button")))
            {

                gc.SaveFlow();
            }
            if (GUILayout.Button(new GUIContent("Remove current flow", "You should never remove last flow. If you do it will create noname emply flow for you")))
            {
                gc.RemoveFlowByIndex(flowIndex);
                GraphWindow.Init();
            }      
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New flow name:");
            newFlowName = EditorGUILayout.TextField(newFlowName);
            if (GUILayout.Button(new GUIContent("Create flow", "Creates new flow if desired name is not used by another flow at this moment")))
            {
                //gc.SaveFlow();

                GraphDataBase gd = new GraphDataBase();
                gd.Name = newFlowName;                
                if (newFlowName.Length > 0)
                {

                    gc.SaveFlow(gd);
                    gc.LoadFlow(newFlowName);
                    
                    //flowDictionary.Add(newFlowName, gd);
                    //DataStore.SaveStorage(DataStore.BlobNames.flow);

                    //componentSource.SetSelectedFlowByLast();                    
                }
                GraphWindow.Init();
            }
        EditorGUILayout.EndHorizontal();
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);           
		}
        
	}
	
    private string[] GetFlowList()
    {

        return GraphComponent.GetInstance().GetFlowArray();


        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;
        string[] ret;

        if (flowDictionary.Length() < 1)
        {
            ret = new string[1];
            ret[0] = "none";
            return ret;
        }

        ret = new string[flowDictionary.Length()];
        for(int i=0; i<flowDictionary.Length(); i++)
        {
            string name;
            ISerializable data;            
            flowDictionary.Get(i, out name, out data);
            ret[i] = name;
        }

        return ret;
    }


	/// <summary>
	/// Helper function to draw button in enabled or disabled state.
	/// </summary>
	static bool DrawButton (string title, string tooltip, bool enabled, float width)
	{
		if (enabled)
		{
			// Draw a regular button
			return GUILayout.Button(new GUIContent(title, tooltip), GUILayout.Width(width), GUILayout.Height(24));
		}
		else
		{
			// Button should be disabled -- draw it darkened and ignore its return value
			Color color = GUI.color;
			GUI.color = new Color(1f, 0.33f, 0.33f, 0.35f);
			GUILayout.Button(new GUIContent(title, tooltip), GUILayout.Width(width), GUILayout.Height(24));
			GUI.color = color;
			return false;
		}
	}
}
