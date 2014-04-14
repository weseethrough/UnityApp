using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;

/// <summary>
/// Simple activity editor window taking care of saving and loading activities from activity blob
/// </summary>
public class ActivityEditorWindow : EditorWindow
{
    static string assetsPath = "ActivityComponents";
	private GameObject[] activitis = {};
    private string[] activitiNames = { };
	private int index = 0;

    /// <summary>
    /// default static unity function called to show window using window type reference
    /// </summary>
    /// <returns></returns>
    [MenuItem("Race Yourself/Activity Editor Window")]	
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(ActivityEditorWindow));
	}

    /// <summary>
    /// draws all options available on activity editor. Mainly load and save option and list of curently managed activities
    /// </summary>
    /// <returns></returns>
    void OnGUI()
    {

        GUILayout.Label("Activity Manager Settings", EditorStyles.boldLabel);
        if (DataStore.instance == null)
        {            
            Debug.Log("Data storage not initialized!");
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
            DataStore.SaveStorage(DataStore.BlobNames.activity);
        }
    }
	
	/// <summary>
	/// prepares list of all known activities for editor display
	/// </summary>
	/// <returns></returns>
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

    /// <summary>
    /// write to memory blob all required activity data
    /// </summary>
    /// <returns></returns>
    void UpdateBlobData()
    {
        Storage s = DataStore.GetStorage(DataStore.BlobNames.activity);
        s.dictionary = new StorageDictionary();
        foreach (GameObject activity in activitis)
        {
            SerializableSettingsBase ss = new SerializableSettingsBase(activity);
            s.dictionary.Add(activity.name, ss);
        }
    }
}
