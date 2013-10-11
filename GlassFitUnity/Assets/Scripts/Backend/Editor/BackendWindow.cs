using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;

public class BackendWindow : EditorWindow
{
    static private string NEW_SCREEN = "New Screen";
	private string[] screens = {NEW_SCREEN};
	private int index = 0;
    private string screenName = "require reload";
    private bool foreceRefresh = false;
	
	[MenuItem("Race Yourself/Editor Tools")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(BackendWindow));
	}

	void OnGUI()
	{ 
		GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
        if (DataStorage.instance == null)
        {
            return;
        }
        		
		int newIndex = EditorGUILayout.Popup(index, screens);
        if (foreceRefresh || 
            newIndex != index)
        {
            index = newIndex;
            screenName = screens[index];
            LoadSavedScreenStructure(index);
            foreceRefresh = false;
        }		
		
		GUILayout.BeginHorizontal();
			GUILayout.Label("ScreenName");
			screenName = GUILayout.TextField(screenName);
		GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
			if (GUILayout.Button("Delete screen"))
            {
                Storage s = DataStorage.GetCoreStorage();
                StorageDictionary screensDictionary = (StorageDictionary)s.dictionary.Get(UIManager.UIPannels);

                if (screensDictionary == null)
                {
                    screensDictionary = new StorageDictionary();
                    s.dictionary.Add(UIManager.UIPannels, screensDictionary);
                }

                if (index < screensDictionary.Length())
                {
                    screensDictionary.Remove(screens[index]);
                }               
                BuildScreenList();
                foreceRefresh = true;
            }	
			if (GUILayout.Button("New screen"))
            {
                UIManager script = (UIManager)FindObjectOfType(typeof(UIManager));
                Transform root = script.transform;
                ClearCurrentStage(root);
                index = screens.Length - 1;
                foreceRefresh = true;
            }
            
            if (GUILayout.Button("Rename to") )
            {
                SaveScreen(true);
            }
            if (GUILayout.Button("Clone to"))
            {
                SaveScreen(false);
            }             
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                SaveScreen(false);
                DataStorage.SaveCoreStorage();
                RefreshFromSource();
            }
            if (GUILayout.Button("Load"))
            {
                DataStorage.instance.Initialize();
                RefreshFromSource();
            }        
        GUILayout.EndHorizontal();
	}
	
	void BuildScreenList()
	{
		Storage s = DataStorage.GetCoreStorage();

        StorageDictionary screensDictionary = (StorageDictionary)s.dictionary.Get(UIManager.UIPannels);

        int count = screensDictionary == null ? 0 : screensDictionary.Length();
        screens = new string[count + 1];
        screens[count] = NEW_SCREEN;

        for (int i = 0; i < count; i++)
        {
            string screenName;
            ISerializable screen;
            screensDictionary.Get(i, out screenName, out screen);
            screens[i] = screenName;
        }
	}

    ISerializable GetCurrentScreenStructure()
    {
        UIManager[] scripts = (UIManager[])FindObjectsOfType(typeof(UIManager));

        if (scripts.Length < 1)
        {
            Debug.LogError("Scene requires to have UIManager in its root");
            return null;
        }
        else if (scripts.Length > 1)
        {
            Debug.LogError("More than one object with UIManager script on it. It is expected to use only one manager as a root point for UI");
        }

        return scripts[0].SaveScene();
    }

    void LoadSavedScreenStructure(int index)
    {
        UIManager script = (UIManager)FindObjectOfType(typeof(UIManager));
        Storage s = DataStorage.GetCoreStorage();
        StorageDictionary screensDictionary = (StorageDictionary)s.dictionary.Get(UIManager.UIPannels);
        if (script == null || screensDictionary == null)
        {
            Debug.LogError("Scene requires to have UIManager in its root");                
        }            
        else
        {
            ISerializable data;
            string screenName;
            screensDictionary.Get(index, out screenName, out data);
            if (data != null)
            {
                ClearCurrentStage(script.transform);
                script.LoadScene((SerializedNode)data);
            }
        }
    }

    void ClearCurrentStage(Transform root)
    {
        foreach (Transform t in root)
        {
            //if this child is serializable clean it up
            if (t.gameObject.GetComponent<UISerializable>() != null)
            {
                GameObject.DestroyImmediate(t.gameObject);
            }
            //if any children contains serialization script we need to clean them up
            else if (t.gameObject.GetComponentInChildren<UISerializable>() != null)
            {
                ClearCurrentStage(t);
            }
        }
    }

    void RefreshFromSource()
    {
        Storage s = DataStorage.GetCoreStorage();
        StorageDictionary screensDictionary = (StorageDictionary)s.dictionary.Get(UIManager.UIPannels);

        BuildScreenList();
        int id = screensDictionary.GetIndex(screenName);
        if (id > -1)
        {
            index = id;
        }
        else
        {
            index = screens.Length - 1;
        }
        screenName = screens[index];
    }

    void SaveScreen(bool deleteOld)
    {
        Storage s = DataStorage.GetCoreStorage();
        StorageDictionary screensDictionary = (StorageDictionary)s.dictionary.Get(UIManager.UIPannels);

        if (screensDictionary == null)
        {
            screensDictionary = new StorageDictionary();
            s.dictionary.Add(UIManager.UIPannels, screensDictionary);
        }

        if (screenName.Length == 0)
        {
            Debug.LogError("You cant set name to empty string! It is recommended to use meaningful identifiers");
        }
        else if (deleteOld && index < screensDictionary.Length())
        {
            screensDictionary.Rename(screens[index], screenName);
        }
        else
        {
            ISerializable structure = GetCurrentScreenStructure();
            if (structure != null)
            {
                screensDictionary.Set(structure, screenName);
            }
        }
        BuildScreenList();

        int id = screensDictionary.GetIndex(screenName);
        if (id > -1)
        {
            index = id;
        }
        else
        {
            index = screens.Length - 1;
        }
    }
}
