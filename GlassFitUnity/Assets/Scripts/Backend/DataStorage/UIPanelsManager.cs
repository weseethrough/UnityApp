using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

using RaceYourself.Models.Blob;
using Sqo;

/// <summary>
/// storage class is simply wrapping class around storage dictionary which allows to plug and unplug easily new classes without losing reference to this root point in places which uses it directly
/// </summary>
[System.Serializable]
public class UIPanelsManager
{
    protected Dictionary<SerializedNodeBase> panels;

    static private UIPanelsManager instance;

    static public SerializedNodeBase GetPanel(string name)
    {
        return GetInstance().panels.Get(name);
    }

    static public Dictionary<SerializedNodeBase> GetPanels()
    {
        return GetInstance().panels;
    }

    static public UIPanelsManager GetInstance()
    {
        if (instance == null)
        {            
            instance = new UIPanelsManager();
            instance.LoadUIPanels();

            //override curent state with old ui panels
        /*    Storage storage = DataStore.GetStorage(DataStore.BlobNames.ui_panels);

            if (storage != null)
            {
                for (int i = 0; i < storage.dictionary.Length(); i++)
                {
                    ISerializable data;
                    string name;

                    storage.dictionary.Get(i, out name, out data);

                    if (!(data is SerializedNodeBase))
                    {
                        Debug.LogError("Panel which is not SerializedNodeBase found during panel list creation");
                    }

                    instance.panels.Add(name, data as SerializedNodeBase);
                }

                instance.panels.name = DataStore.BlobNames.ui_panels.ToString();
            }   */
            //old override
        }
        return instance;

    }

    public void LoadUIPanels()
    {
        float startTime = Time.realtimeSinceStartup;        

        Siaqodb db = SiaqodbUtils.DatabaseFactory.GetStaticInstance();
        ISqoQuery<Dictionary<SerializedNodeBase>> q = db.Query<Dictionary<SerializedNodeBase>>();
        Dictionary<SerializedNodeBase> data = q.Where<Dictionary<SerializedNodeBase>>(r => r.name == DataStore.BlobNames.ui_panels.ToString()).First();

        if (data == null)
        {
            data = new Dictionary<SerializedNodeBase>();
            data.name = DataStore.BlobNames.ui_panels.ToString();
        }

        panels = data;

        float endTime = Time.realtimeSinceStartup;
        Debug.Log("Loading time for uiPanels took " + (float)(endTime - startTime));

    }

    public void SaveUIPanels()
    {
        if (panels == null) return;
        

        Siaqodb db = SiaqodbUtils.DatabaseFactory.GetStaticInstance();        
        db.StoreObject(panels);
    }
}
