using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using System.IO;

/// <summary>
/// Main Ui management editor window
/// </summary>
public class UIEditorWindow : EditorWindow
{
	enum GUILayer
	{
		unknown,
		e2D,
		e3D
	}

	static private string stageRoot = "UIComponents/StageRoot";

    static private string NEW_SCREEN = "New Screen";
	private string[] screens = {NEW_SCREEN};
	private int index = 0;
    private string screenName = "require reload";
    private bool foreceRefresh = false;
	private Vector2 panelScroller = new Vector2();

    /// <summary>
    /// default static unity function called to show window using window type reference
    /// </summary>
    /// <returns></returns>
    [MenuItem("Race Yourself/UI Editor Window")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
        EditorWindow window = EditorWindow.GetWindow(typeof(UIEditorWindow));        
	}

	/// <summary>
	/// advanced ui system allows for copying, modifying, naming and saving panels for further usage in flow editor
	/// </summary>
	/// <returns></returns>
	void OnGUI()
	{ 
		GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
        if (DataStore.instance == null)
        {
            Debug.Log("Data storage not initialized!"); 
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
                StorageDictionary screensDictionary = Panel.GetPanelDictionary();

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
                DataStore.SaveStorage(DataStore.BlobNames.ui_panels, true);
                RefreshFromSource();

				if (UIManager.panelData.Count < 2)
				{
					UIManager.LoadPanelData();
				}
				UIManager.SavePanelData();
            }
            if (GUILayout.Button("Load"))
            {
                DataStore.instance.Initialize();
                RefreshFromSource();
                foreceRefresh = true;

				UIManager.LoadPanelData();
            }        
        GUILayout.EndHorizontal();

		panelScroller = GUILayout.BeginScrollView(panelScroller);

        bool refresh = false;
		foreach (string name in UIManager.panelList)
        {
            UnityEngine.Object obj = null;
            GUILayout.BeginHorizontal();                
                obj = EditorGUILayout.ObjectField( obj, typeof(GameObject), GUILayout.MaxWidth(35.0f));
                if ( GUILayout.Button("e", GUILayout.MaxWidth(20.0f)))
			    {
					if (UIManager.panelData.ContainsKey(name))
					{
						EditPanel(name, UIManager.panelData[name]);
					}
				}
				GUI.color = Color.red;
				if ( GUILayout.Button("x", GUILayout.MaxWidth(20.0f)))
				{
					UIManager.RemovePanel(name);
				}
				GUI.color = Color.white;
				
				GUILayout.Label(name, GUILayout.MinWidth(150.0f));
               // GUILayout.Label("path:", GUILayout.MaxWidth(60.0f));
		
				string newName = name; 
				if (obj is GameObject)
				{
					string path = AssetDatabase.GetAssetPath(obj);
					UIManager.panelData[name] = SerializedNode.GetResourcePath(path);
					newName = obj.name;
					refresh = true;
				}

                if (newName != name)
                {
					if (UIManager.panelData.ContainsKey(name))
                    {
					UIManager.panelData[newName] = GUILayout.TextField(UIManager.panelData[name], GUILayout.MaxWidth(150.0f));
                    }
                    else
                    {
					UIManager.panelData[newName] = GUILayout.TextField("", GUILayout.MaxWidth(150.0f));
                    }

					UIManager.panelData.Remove(name);
                    refresh = true;
                }
				else if (UIManager.panelData.ContainsKey(name))
                {
					string path = GUILayout.TextField(UIManager.panelData[name], GUILayout.MaxWidth(150.0f));
					if ( UIManager.panelData[name] != path)
					{
						UIManager.panelData[name] = path;
						refresh = true;
					}
                }
                else
                {
					UIManager.panelData[name] = GUILayout.TextField("", GUILayout.MaxWidth(150.0f));
                }
                
            GUILayout.EndHorizontal();
        }

		GUILayout.EndScrollView();

        if (GUILayout.Button("+"))
        {
			if (UIManager.panelData.Count < 2)
			{
				UIManager.LoadPanelData();
			}
			UIManager.panelData.Add("!!NoName"+UIManager.panelData.Count, "");
            UIManager.BuildList();
			UIManager.SavePanelData();
        }
		else if (refresh)
		{
			UIManager.BuildList();
			UIManager.SavePanelData();
		}
        
	}
	
	/// <summary>
	/// prepares list of all stored in blob screens. Adds "new screen" option for ease of adding new panels
	/// </summary>
	/// <returns></returns>
	void BuildScreenList()
	{
        StorageDictionary screensDictionary = Panel.GetPanelDictionary();

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

    /// <summary>
    /// finds manager and saves to the blob currently selected ui scene
    /// </summary>
    /// <returns>root point of current screen serializable structure</returns>
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

    /// <summary>
    /// loads screens from blob
    /// </summary>
    /// <param name="index">index of the screen of the interest</param>
    /// <returns></returns>
    void LoadSavedScreenStructure(int index)
    {
        UIManager script = (UIManager)FindObjectOfType(typeof(UIManager));
        StorageDictionary screensDictionary = Panel.GetPanelDictionary();

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

	void EditPanel(string panelName, string path)
	{
		UIManager script = (UIManager)FindObjectOfType(typeof(UIManager));

		if (script == null)
		{
			Debug.LogError("Scene requires to have UIManager in its root");                
		}            
		else
		{
			ClearCurrentStage(script.transform);
			GameObject stage = PrefabUtility.InstantiatePrefab(Resources.Load(stageRoot)) as GameObject;
            GameObject source = Resources.Load(path)as GameObject;

			stage.transform.parent = script.transform;

			GUILayer l = GetTransformLayer(source.transform);
			script.LoadPrefabPanel(panelName, GetContainerName(l), stage);
		}
	}

	string GetContainerName(GUILayer layer)
	{
		switch (layer)
		{
		case GUILayer.e3D:
			return "Widgets Container3D";
		case GUILayer.e2D:
		default:
			return "Widgets Container";
		}
	}

	GUILayer GetTransformLayer(Transform root)
	{
		int layer = root.gameObject.layer; 
		if (layer == LayerMask.NameToLayer("GUI"))
		{
			return GUILayer.e2D;
		}
        else if (layer == LayerMask.NameToLayer("GUI3D"))
		{
			return GUILayer.e3D;
		}

		foreach (Transform t in root)
		{
			GUILayer l = GetTransformLayer(t);
			if (l != GUILayer.unknown)
			{
				return l;
			}
		}

		return GUILayer.unknown;
	}

    /// <summary>
    /// clears current ui structure
    /// </summary>
    /// <param name="root">root point containing children with uiserialziables</param>
    /// <returns></returns>
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

    /// <summary>
    /// reload/rebuild screen from blob
    /// </summary>
    /// <returns></returns>
    void RefreshFromSource()
    {
        StorageDictionary screensDictionary = Panel.GetPanelDictionary();

        BuildScreenList();
        int id = screensDictionary != null ? screensDictionary.GetIndex(screenName) : -1;
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

    /// <summary>
    /// saves screen with possibility to delete old name it were existing with
    /// </summary>
    /// <param name="deleteOld">is this rename instead of clone?</param>
    /// <returns></returns>
    void SaveScreen(bool deleteOld)
    {
        //Storage s = DataStore.GetStorage(DataStore.BlobNames.ui_panels);
        StorageDictionary screensDictionary = Panel.GetPanelDictionary(false);

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
                screensDictionary.Set(screenName, structure);
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
