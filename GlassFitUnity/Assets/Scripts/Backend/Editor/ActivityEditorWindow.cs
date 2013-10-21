using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;

public class ActivityEditorWindow : EditorWindow
{
    static string assetsPath = "ActivityComponents";
	private GameObject[] activitis = {};
    private string[] activitiNames = { };
	private int index = 0;    

    [MenuItem("Race Yourself/Activity Editor Window")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(ActivityEditorWindow));
	}

    void OnGUI()
    {

        GUILayout.Label("Activity Manager Settings", EditorStyles.boldLabel);
        if (DataStorage.instance == null)
        {
            DataStorage ds = (DataStorage)GameObject.FindObjectOfType(typeof(DataStorage));
            if (ds != null && GUILayout.Button("Initialize"))
            {
                ds.MakeAwake();
            }
            return;
        }

        GUILayout.Label("List of loaded activities:");
        index = EditorGUILayout.Popup(index, activitiNames);

        if (GUILayout.Button("Load activities from assets"))
        {
            BuildActivityList();
        }
        if (GUILayout.Button("Save activities to blob"))
        {
            UpdateBlobData();
            DataStorage.SaveStorage(DataStorage.BlobNames.activity);
        }
    }
	
	void BuildActivityList()
	{
        UnityEngine.Object[] assets = Resources.LoadAll(assetsPath);
        
        activitis = new GameObject[assets.Length];
        activitiNames = new string[assets.Length];
        for (int i =0; i< assets.Length; i++)
        {
            GameObject go = assets[i] as GameObject;
            activitis[i] = go;
            if (go != null)
            {
                activitiNames[i] = go.name;
            }
            else
            {
                activitiNames[i] = "Object is not valid";
            }
            
            
        }        
	}

    void UpdateBlobData()
    {
        Storage s = DataStorage.GetStorage(DataStorage.BlobNames.activity);
        s.dictionary = new StorageDictionary();
        foreach (GameObject activity in activitis)
        {
            SerializableSettings ss = new SerializableSettings(activity);
            s.dictionary.Add(activity.name, ss);
        }
    }
}
