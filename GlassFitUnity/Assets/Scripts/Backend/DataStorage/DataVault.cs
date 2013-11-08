using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class DataEntry
{        
    public System.Object   storedValue;    
    public bool            persistent;

    public DataEntry(System.Object v, bool p)
    {
        storedValue = v;
        persistent  = p;
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
    

    void Start()
    {
        data = new Dictionary<string, DataEntry>();
        registeredListeners = new Dictionary<string, List<UIComponentSettings>>();
        registrationRecord = new Dictionary<UIComponentSettings, List<string>>();
        Initialize();
    }

    static public void SaveToBlob()
    {
        Storage s = DataStore.GetStorage(DataStore.BlobNames.persistent);
        if (s == null) return;

        //process all stored types and write them into 
        StorageDictionaryBase<int> intStorage       = new StorageDictionaryBase<int>();
        StorageDictionaryBase<bool> boolStorage     = new StorageDictionaryBase<bool>();
        StorageDictionaryBase<double> doubleStorage = new StorageDictionaryBase<double>();        
        StorageDictionaryBase<string> stringStorage = new StorageDictionaryBase<string>();

        foreach (var pair in data)
        {
            DataEntry de = pair.Value;
            if (de.persistent)
            {
                if (de.storedValue.GetType() == typeof(int))
                {
                    intStorage.Set(Convert.ToInt32(de.storedValue), pair.Key);
                }
                else if (de.storedValue.GetType() == typeof(bool))
                {
                    boolStorage.Set(Convert.ToBoolean(de.storedValue), pair.Key);
                }
                else if (de.storedValue.GetType() == typeof(double))
                {
                    doubleStorage.Set(Convert.ToDouble(de.storedValue), pair.Key);
                }
                else if (de.storedValue.GetType() == typeof(string))
                {
                    stringStorage.Set(Convert.ToString(de.storedValue), pair.Key);
                }
            }
        }

        s.dictionary.Set(intStorage, Types.Integer.ToString());
        s.dictionary.Set(doubleStorage, Types.Double .ToString());
        s.dictionary.Set(boolStorage, Types.Boolean .ToString());
        s.dictionary.Set(stringStorage, Types.String .ToString());

        DataStore.SaveStorage(DataStore.BlobNames.persistent);
    }

    static void Initialize()
    {
        Storage s = DataStore.GetStorage(DataStore.BlobNames.persistent);
        if (s == null) return;

        //process all stored types and write them into 
        StorageDictionaryBase<int> intStorage = s.dictionary.Get(Types.Integer.ToString()) as StorageDictionaryBase<int>;
        if (intStorage != null)
        {
            for (int i = 0; i < intStorage.Length(); i++)
            {
                string name;
                int value;
                intStorage.Get(i, out name, out value);
                if (name.Length > 0)
                {
                    data[name] = new DataEntry(value, true);
                }
            }
        }

        StorageDictionaryBase<bool> boolStorage = s.dictionary.Get(Types.Boolean.ToString()) as StorageDictionaryBase<bool>;
        if (intStorage != null)
        {
            for (int i = 0; i < boolStorage.Length(); i++)
            {
                string name;
                bool value;
                boolStorage.Get(i, out name, out value);
                if (name.Length > 0)
                {
                    data[name] = new DataEntry(value, true);
                }
            }
        }

        StorageDictionaryBase<double> doubleStorage = s.dictionary.Get(Types.Double.ToString()) as StorageDictionaryBase<double>;
        if (doubleStorage != null)
        {
            for (int i = 0; i < doubleStorage.Length(); i++)
            {
                string name;
                double value;
                doubleStorage.Get(i, out name, out value);
                if (name.Length > 0)
                {
                    data[name] = new DataEntry(value, true);
                }
            }
        }

        StorageDictionaryBase<string> stringStorage = s.dictionary.Get(Types.String.ToString()) as StorageDictionaryBase<string>;
        if (stringStorage != null)
        {
            for (int i = 0; i < stringStorage.Length(); i++)
            {
                string name;
                string value;
                stringStorage.Get(i, out name, out value);
                if (name.Length > 0)
                {
                    data[name] = new DataEntry(value, true);
                }
            }
        }                
    }

    static public void Set(string name, System.Object value)
    {
        if (data == null) 
        {
            Debug.LogError("Database is not initialized yet, value set will be ignored!");
            return;
        }

        DataEntry de;

        if (data.ContainsKey(name))
        {
            de = data[name];

            //ensure we alert when someone is changing type of stored value
            if ((de.storedValue.GetType() != typeof(System.Object)) &&
                (de.storedValue.GetType() != value.GetType()))
            {
                Debug.LogError("Trying to change type of stored variable. It is a bad practice to do so. If you did not chnged type maybe its the system wrongly processing data provided?");
            }

            de.storedValue = value;
        }
        else
        {
            data[name] = new DataEntry(value, false);
        }

        if (registeredListeners.ContainsKey(name))
        {
            List<UIComponentSettings> list = registeredListeners[name];
            if (list != null)
            {
                foreach (UIBasiclabel listener in list)
                {
                    listener.SetTranslatedText(false);
                }
            }
        }                
    }

    static public void SetPersistency(string name, bool value)
    {
        if (data == null)
        {
            Debug.LogError("Database is not initialized yet, value set will be ignored!");
            return;
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

    static public System.Object Get(string name)
    {
        if (data == null)
        {
            return null;
        }

        if (!data.ContainsKey(name))
        {
            return null;
        }

        return data[name].storedValue;
    }

    static public void Remove(string name)
    {
        if (data == null || !data.ContainsKey(name))
        {            
            return;
        }

        data.Remove(name);
    }

    static public string Translate(string source, UIComponentSettings registerForUpdates)
    {
        return Translate(source, 0, registerForUpdates);
    }

    static public string Translate(string source, int startingPoint, UIComponentSettings registerForUpdates)
    {
        if (startingPoint >= source.Length) return source;

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
                string startSection = start > 0 ? source.Substring(0,start) : "";
                string endSection = end < source.Length-1 ? source.Substring(end+1,source.Length-end) : "";
                return startSection + newSection + endSection;
            }
        }

        return source;
    }

    static public void RegisterListner(UIComponentSettings listner, string identifier)
    {
        if (registeredListeners != null && registrationRecord != null &&
            listner != null && identifier.Length > 0)
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
