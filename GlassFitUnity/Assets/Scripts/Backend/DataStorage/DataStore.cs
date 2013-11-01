using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

[ExecuteInEditMode]
public class DataStore : MonoBehaviour 
{    
	static public DataStore _instance;
    static public DataStore instance
    {
        get {
                if (_instance != null)
                {
                    return _instance;
                }
                DataStore ds = (DataStore)GameObject.FindObjectOfType(typeof(DataStore));
                if (ds != null)
                {
                    ds.MakeAwake();
                }
                return _instance;
            }        
    }
		
    //public properties
	//Note! blob names are used by machine to define file names. Ensure any names used are standard characters for file name.
    //names are created from enum which if changed would lose pointer to saved on drive blob

    public enum BlobNames
    {
        ui_panels,
        activity,
        flow,
        persistent,        
        maxItem
    }

    //private properties
    private Dictionary<string, Storage> storageBank;

#if UNITY_EDITOR
	private PlatformDummy platform;
#else
    private Platform platform;
#endif

    void Awake()
    {
        MakeAwake();        
    }

    public void MakeAwake()
    {        
        _instance = this;

        if (platform != null)
        {
            Debug.LogWarning("Reinitialziation of datastorage canceled");
            return;
        }
#if UNITY_EDITOR
        platform = new PlatformDummy();
#else
        platform = new Platform();
#endif
		storageBank = new Dictionary<string, Storage>();
		
        Initialize();
    }   

    public void Initialize()
    {
        
        for (int i = 0; i < (int)BlobNames.maxItem; i++ )
        {
            BlobNames bName = (BlobNames)i;
            string name = bName.ToString();
            storageBank[name] = InitializeBlob(platform.LoadBlob(name));
            
            Debug.Log("-A------------START------------A-");
            StorageDictionary sd = storageBank[name].dictionary;

            Debug.Log("Storage "+name+" contains "+ sd.Length() +" elements");
            
            for (int k = 0; k < sd.Length(); k++)
            {
                string n;
                ISerializable d;                
                sd.Get(k, out n, out d);
                Debug.Log(k+ ": " + n);
            }
            Debug.Log("-A------------END--------------A-");
        }        
	}
	
	private Storage InitializeBlob(byte[] source)
	{
		/* test code */
		
		if (source == null)
		{
			Debug.LogWarning("Blob doesn't exist, we are creating one");
			return new Storage();			
		}
		
		Storage storage;
		
		try 
		{
			MemoryStream ms = new MemoryStream(source);	
			ms.Position = 0;
	
	        BinaryFormatter bformatter = new BinaryFormatter();
	        System.Object o = bformatter.Deserialize(ms);
	        storage = (Storage)o;
		}
		catch
		{
			return new Storage();
		}
        return storage;		
	}
	
	static public Storage GetStorage(BlobNames name)
	{
		if (instance != null)
		{
            return instance.storageBank[name.ToString()];	
		}
		
		return null;
	}

    static public void LoadStorage(BlobNames name)
    {
        if (instance != null && instance.platform != null)
        {
            instance.storageBank[name.ToString()] = instance.InitializeBlob(instance.platform.LoadBlob(name.ToString()));
        }
    }

    static public void SaveStorage(BlobNames name)
    {
        if (instance != null && instance.platform != null)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(ms, GetStorage(name));

            instance.platform.StoreBlob(name.ToString(), ms.GetBuffer());
#if UNITY_EDITOR
            instance.platform.StoreBlobAsAsset(name.ToString(), ms.GetBuffer());
#endif
        }
    }
}