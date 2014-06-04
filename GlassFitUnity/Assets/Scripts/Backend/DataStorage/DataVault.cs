using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Sqo;

[System.Serializable]
public class DataEntry
{
    //warning some objects might not be serializable!
    //If you use this class for serialization process ensure it does check on stored value
    public System.Object storedValue;
    public bool persistent;

    public DataEntry(System.Object v, bool p)
    {
        storedValue = v;
        persistent = p;
    }
}


/// <summary>
/// Class used to centralize access for variables. Is working on top on DataStorage
/// </summary>
[ExecuteInEditMode]
public class DataVault : MonoBehaviour
{
    enum Types
    {
        Integer,
        Boolean,
        Double,
        String,
    }

    const string STARTING_BRACKET = "<db_";
    const string ENDING_BRACKET = ">";

    static public Dictionary<string, DataEntry> data;
    static public Dictionary<string, List<UIComponentSettings>> registeredListeners;
    static public Dictionary<UIComponentSettings, List<string>> registrationRecord;


    /// <summary>
    /// default unity initialziation function is preparing datavault and loads variables to useful easy to search dictionaries
    /// </summary>
    /// <returns></returns>
    void Start()
    {
        Initialize(true);
    }

    /// <summary>
    /// saves persistent data into external blob data
    /// </summary>
    /// <returns></returns>
    static public void SaveToBlob()
    {
        Siaqodb db = SiaqodbUtils.DatabaseFactory.GetStreamingInstance();

        ISqoQuery<RaceYourself.Models.Blob.PersistentData> q = db.Query<RaceYourself.Models.Blob.PersistentData>();

        RaceYourself.Models.Blob.PersistentData vault = null;
        if (q.Count() > 0)
        {
            vault = q.First();
        }
        else
        {
            vault = new RaceYourself.Models.Blob.PersistentData();
        }

        //load all stored data to local structure (later can be replaced by usage of remote type only
        foreach (KeyValuePair<string, DataEntry> de in data)
        {
            DataEntry dEntry = de.Value;

            if (dEntry.persistent == true)
            {
                vault.AddData(de.Key, dEntry.storedValue);
            }
        }

        db.StoreObject(vault);
        return;        
    }

    /// <summary>
    /// loads data form current datastore and prepares easy to search dictionaries
    /// </summary>
    /// <returns></returns>
    static public void Initialize()
    {
        Initialize(false);
    }

    /// <summary>
    /// loads data form current datastore and prepares easy to search dictionaries
    /// </summary>
    /// <param name="forced"></param>
    /// <returns></returns>
    static public void Initialize(bool forced)
    {

        if (data != null && !forced) return;

        registeredListeners = new Dictionary<string, List<UIComponentSettings>>();
        registrationRecord = new Dictionary<UIComponentSettings, List<string>>();

        data = new Dictionary<string, DataEntry>();

        Siaqodb db = SiaqodbUtils.DatabaseFactory.GetStreamingInstance();
        ISqoQuery<RaceYourself.Models.Blob.PersistentData> q = db.Query<RaceYourself.Models.Blob.PersistentData>();

        RaceYourself.Models.Blob.PersistentData vault = q.FirstOrDefault();

        //load all stored data to local structure (later can be replaced by usage of remote type only
        if (vault != null)
        {
            foreach (RaceYourself.Models.Blob.DictionaryEntry<int> val in vault.listInt)
            {
                data[val.name] = new DataEntry(val.data, true);
            }

            foreach (RaceYourself.Models.Blob.DictionaryEntry<bool> val in vault.listBool)
            {
                data[val.name] = new DataEntry(val.data, true);
            }

            foreach (RaceYourself.Models.Blob.DictionaryEntry<double> val in vault.listDouble)
            {
                data[val.name] = new DataEntry(val.data, true);
            }

            foreach (RaceYourself.Models.Blob.DictionaryEntry<string> val in vault.listStr)
            {
                data[val.name] = new DataEntry(val.data, true);
            }


            return;
        }
    }

    /// <summary>
    /// sets one value to one of the dictionaries
    /// </summary>
    /// <param name="name">name identifier to the value</param>
    /// <param name="value">float, double, int, bool or string which would get set under name in dictionaries</param>
    /// <returns></returns>
    static public void Set(string name, System.Object value)
    {
        if (data == null)
        {
            Initialize();
        }

        DataEntry de;

        if (data.ContainsKey(name))
        {
            de = data[name];

            //ensure we alert when someone is changing type of stored value
            Type stored = de.storedValue.GetType();
            Type newValue = value.GetType();


            if ((stored != typeof(System.Object)) &&
                (newValue != stored) &&
                !((newValue == typeof(double) && stored == typeof(float)) || (newValue == typeof(float) && stored == typeof(double))) //its not double to float casting by any chance?
                )
            {
                Debug.LogError("Trying to change type of stored variable. It is a bad practice to do so. If you did not chnged type maybe its the system wrongly processing data provided?");
            }

            de.storedValue = value;
        }
        else
        {
            data[name] = new DataEntry(value, false);
        }

        if (Application.isPlaying && registeredListeners.ContainsKey(name))
        {
            List<UIComponentSettings> list = registeredListeners[name];
            if (list != null)
            {
                foreach (UIComponentSettings listener in list)
                {
                    listener.Apply();// .SetTranslatedText(false);
                }
            }
        }
    }

    /// <summary>
    /// sets perisitency flag to single variable in dictionary
    /// </summary>
    /// <param name="name">name of the variabe to have flach changed</param>
    /// <param name="value">new flag state</param>
    /// <returns></returns>
    static public void SetPersistency(string name, bool value)
    {
        if (data == null)
        {
            Initialize();
        }

        if (!data.ContainsKey(name))
        {
            Debug.LogError("Trying to set persistency on name which doesn't exists: " + name);
            return;
        }

        DataEntry de = data[name];
        Type t = de.storedValue.GetType();
        if (value == true)
        {
            if (t == typeof(int) || t == typeof(bool) || t == typeof(double) || t == typeof(float) || t == typeof(string))
            {
                de.persistent = value;
            }
            else
            {
                Debug.LogWarning("You can't make object or complex types as persistent! " + name + " cannot be saved");
            }
        }
        else
        {
            de.persistent = false;
        }
    }

    /// <summary>
    /// returns instance of the object stored in the vault finding it by name
    /// </summary>
    /// <param name="name">name identifier searched variable</param>
    /// <returns>instance of the value searched</returns>
    static public System.Object Get(string name)
    {
        if (data == null)
        {
            Initialize();
        }

        if (!data.ContainsKey(name))
        {
            return null;
        }

        return data[name].storedValue;
    }

    /// <summary>
    /// removes record of the variable
    /// </summary>
    /// <param name="name">name identifier</param>
    /// <returns></returns>
    static public void Remove(string name)
    {
        if (data == null)
        {
            Initialize();
        }

        if (!data.ContainsKey(name))
        {
            return;
        }

        data.Remove(name);
    }

    /// <summary>
    /// tries to translate component string variable with one of the stored internally in data vault
    /// </summary>
    /// <param name="source">string to get translated</param>
    /// <param name="registerForUpdates">if provided, object would get informed when changes to variables are made those which were used for translations</param>
    /// <returns>translated values</returns>
    static public string Translate(string source, UIComponentSettings registerForUpdates)
    {
        return Translate(source, 0, registerForUpdates);
    }

    /// <summary>
    /// tries to translate component string variable with one of the stored internally in data vault
    /// </summary>
    /// <param name="source">string to get translated</param>
    /// <param name="startingPoint">offset search would start from</param>
    /// <param name="registerForUpdates">if provided, object would get informed when changes to variables are made those which were used for translations</param>
    /// <returns>translated values</returns>    
    static public string Translate(string source, int startingPoint, UIComponentSettings registerForUpdates)
    {
        if (!Application.isPlaying || startingPoint >= source.Length) return source;

        int start = source.IndexOf(STARTING_BRACKET, startingPoint);
        int end;

        if (start > -1)
        {
            end = source.IndexOf(ENDING_BRACKET, start);
            if (end > -1)
            {
                source = Translate(source, end, registerForUpdates);

                int dataStart = start + STARTING_BRACKET.Length;
                string dataName = source.Substring(dataStart, end - dataStart);
                string newSection = "";
                if (dataName.Length > 0)
                {
                    RegisterListner(registerForUpdates, dataName);

                    //find translated word
                    System.Object obj = Get(dataName);

                    if (obj == null)
                    {
                        //we did not found word, we will return unchanged
                        return source;
                    }
                    else
                    {
                        //we have found word
                        newSection = obj.ToString();
                    }

                }
                string startSection = start > 0 ? source.Substring(0, start) : "";
                string endSection = end < source.Length - 1 ? source.Substring(end + 1, source.Length - end - 1) : "";
                return startSection + newSection + endSection;
            }
        }

        return source;
    }

    /// <summary>
    /// registers component for events under identifier named event
    /// </summary>
    /// <param name="listner">object which would get informed when variable changes</param>
    /// <param name="identifier">iidentofier which would be used for registration, any change to variable with this name would make listner informed</param>
    /// <returns></returns>
    static public void RegisterListner(UIComponentSettings listner, string identifier)
    {
        if (Application.isPlaying &&
            registeredListeners != null && registrationRecord != null &&
            listner != null && identifier != null && identifier.Length > 0)
        {
            if (!registeredListeners.ContainsKey(identifier))
            {
                registeredListeners[identifier] = new List<UIComponentSettings>();
            }

            if (!registeredListeners[identifier].Contains(listner))
            {
                registeredListeners[identifier].Add(listner);
            }

            if (!registrationRecord.ContainsKey(listner))
            {
                registrationRecord[listner] = new List<string>();
            }

            if (!registrationRecord[listner].Contains(identifier))
            {
                registrationRecord[listner].Add(identifier);
            }
        }
    }

    /// <summary>
    /// removes from registration listner list
    /// </summary>
    /// <param name="listner">listrner previously listening for events</param>
    /// <returns></returns>
    static public void UnRegisterListner(UIComponentSettings listner)
    {
        if (registrationRecord != null && registeredListeners != null && listner != null)
        {
            if (registrationRecord.ContainsKey(listner) && registrationRecord[listner] != null)
            {
                List<string> list = registrationRecord[listner];
                foreach (string identifier in list)
                {
                    if (registeredListeners.ContainsKey(identifier) && registeredListeners[identifier] != null)
                    {
                        registeredListeners[identifier].Remove(listner);
                    }
                }
                registrationRecord.Remove(listner);
            }
        }
    }
}
