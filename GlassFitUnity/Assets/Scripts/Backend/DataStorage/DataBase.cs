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
public class DataBase : MonoBehaviour 
{
    enum Types
    {
        Integer,
        Boolean,
        Double,
        String,
    }    

    static public Dictionary<string, DataEntry> data;
    const string STARTING_BRACKET   = "<";
    const string ENDING_BRACKET     = ">";
    const string IDENTIFIER         = "db_";

    void Start()
    {
        data = new Dictionary<string, DataEntry>();
        Initialize();
    }

    static public void SaveToBlob()
    {
        Storage s = DataStorage.GetStorage(DataStorage.BlobNames.persistent);
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

        DataStorage.SaveStorage(DataStorage.BlobNames.persistent);
    }

    static void Initialize()
    {
        Storage s = DataStorage.GetStorage(DataStorage.BlobNames.persistent);
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
            Debug.LogError("Trying to set perisistency on name which doesn't exists: " + name);
            return;
        }

        DataEntry de = data[name];
        de.persistent = value;        
    }

    static public System.Object Get(string name)
    {
        if (data == null)
        {
            Debug.LogError("Database is not initialized yet, value set will be set to devault!");
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

    static public string Translate(string source)
    {
        return Translate(source, 0);
    }

    static public string Translate(string source, int startingPoint)
    {
        if (startingPoint >= source.Length) return source;

        int start = source.IndexOf(STARTING_BRACKET, startingPoint);
        int end;
        int word;
         
        if (start > -1)
        {
            end = source.IndexOf(ENDING_BRACKET, start);
            word = source.IndexOf(IDENTIFIER, start);
            if (end > -1 && word > -1)
            {
                source = Translate(source, end);

                int dataStart = word +IDENTIFIER.Length;
                string dataName = source.Substring(dataStart, end - dataStart);
                string newSection = "";
                if (dataName.Length > 0)
                {
                    System.Object obj = Get(dataName);
                    newSection = obj != null ? obj.ToString() : dataName;
                }
                string startSection = start > 0 ? source.Substring(0,start) : "";
                string endSection = end < source.Length-1 ? source.Substring(end+1,source.Length-end) : "";
                return startSection + newSection + endSection;
            }
        }

        return source;
    }
}
