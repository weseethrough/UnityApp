using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

/// <summary>
/// root data storage class. used for management of all backend and ui external static data
/// </summary>
public class DataStore : MonoBehaviour 
{    
	static public DataStore _instance;
    static public DataStore instance
    {
        get 
        {
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
	public PlatformDummy platform;
#endif

    /// <summary>
    /// initialized class and prepares for further use.
    /// </summary>
    /// <returns></returns>
    public void MakeAwake()
    {
        _instance = this;
#if UNITY_EDITOR
        if (platform != null)
        {
            Debug.LogWarning("Reinitialziation of datastorage canceled");
            return;
        }
//
//		if (Platform.Instance != null)
//        {
//            Debug.LogWarning("Reinitialziation of datastorage canceled");
//            return;
//        }
#endif
		
#if UNITY_EDITOR
        platform = new PlatformDummy();
#endif
		storageBank = new Dictionary<string, Storage>();
		
        Initialize();
    }   
	
	/// <summary>
	/// default unity initialization function. Called to stop this gameobject from destruction when switching between scenes
	/// </summary>	
	void Awake() 
    {
		DontDestroyOnLoad(transform.gameObject);
	}

    /// <summary>
    /// load blobs and prepare them for further usage by different systems
    /// </summary>
    /// <returns></returns>
    public void Initialize()
    {
        //load data blobs from drive
        for (int i = 0; i < (int)BlobNames.maxItem; i++ )
        {
            BlobNames bName = (BlobNames)i;
            string name = bName.ToString();
#if UNITY_EDITOR
            storageBank[name] = InitializeBlob(platform.LoadBlob(name));
#else
			storageBank[name] = InitializeBlob(Platform.Instance.LoadBlob(name));
#endif
            StorageDictionary sd = storageBank[name].dictionary;            
            for (int k = 0; k < sd.Length(); k++)
            {
                string n;
                ISerializable d;                
                sd.Get(k, out n, out d);            
            }            
        }        
	}
	
	/// <summary>
	/// initialzie blob storage instance using byte source loaded form external file or database
	/// </summary>
	/// <param name="source">byte array structural source of the storage</param>
	/// <returns>ready touse sotrage</returns>
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
        catch (Exception e)
		{
            Debug.LogException(e);
			return new Storage();
		}
        return storage;		
	}
	
	/// <summary>
	/// finds storage using enum name
	/// </summary>
	/// <param name="name"></param>
	/// <returns>null if storage doesn't exist or datastore instance is null, or storage instance if one is found</returns>
	static public Storage GetStorage(BlobNames name)
	{
		if (instance != null)
		{
            return instance.storageBank[name.ToString()];	
		}
		
		return null;
	}

    /// <summary>
    /// loads single instance of the blob refresing curent sturcture from external source
    /// </summary>
    /// <param name="name">blob enum name</param>
    /// <returns></returns>
    static public void LoadStorage(BlobNames name)
    {
#if UNITY_EDITOR
        if (instance != null && instance.platform != null)
        {
            instance.storageBank[name.ToString()] = instance.InitializeBlob(instance.platform.LoadBlob(name.ToString()));
        }
#else
		if (instance != null)
        {
            instance.storageBank[name.ToString()] = instance.InitializeBlob(Platform.Instance.LoadBlob(name.ToString()));
        }
#endif
    }

    /// <summary>
    /// saves storage to external data block for later use
    /// </summary>
    /// <param name="name">name of the blob to be saved</param>
    /// <returns></returns>
    static public void SaveStorage(BlobNames name)
    {
#if UNITY_EDITOR
        if (instance != null && instance.platform != null)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(ms, GetStorage(name));

            instance.platform.StoreBlob(name.ToString(), ms.GetBuffer());

            instance.platform.StoreBlobAsAsset(name.ToString(), ms.GetBuffer());
		}
#else
		if (instance != null)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(ms, GetStorage(name));

            Platform.Instance.StoreBlob(name.ToString(), ms.GetBuffer());

     	}
#endif
        
    }
}
