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
                Profiler.BeginSample("_oOo_DataStore_oOo_");
                ds.MakeAwake();
                Profiler.EndSample();
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
        maxItem,
        saves
    }

    //private properties
    private Dictionary<string, Storage> storageBank;
	
//#if UNITY_EDITOR
//	private PlatformDummy platform;
//#endif

    /// <summary>
    /// initialized class and prepares for further use.
    /// </summary>
    /// <returns></returns>
    public void MakeAwake()
    {
        _instance = this;
#if UNITY_EDITOR
//        if (PlatformDummy.Instance != null)
//        {
//            Debug.LogWarning("Reinitialziation of datastorage canceled");
//            return;
//        }
//
//		if (Platform.Instance != null)
//        {
//            Debug.LogWarning("Reinitialziation of datastorage canceled");
//            return;
//        }
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
            if (!LoadStorageFromCollection(bName))
            {
                Profiler.BeginSample("Test profile");
                storageBank[name.ToString()] = InitializeBlob(Platform.Instance.LoadBlob(name));
                Profiler.EndSample();
            }            
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
            if (ms.Length == 0)
            {
                return new Storage();
            }
	        else
            {                
                BinaryFormatter bformatter = new BinaryFormatter();
                System.Object o = bformatter.Deserialize(ms);
                storage = (Storage)o;                
            }	        
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
        if (instance != null && instance.storageBank.ContainsKey(name.ToString()))
		{
			//UnityEngine.Debug.Log("DataStore: Name is " + name);
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
        if (instance != null && Platform.Instance != null)
        {
            
            if (!LoadStorageFromCollection(name))
            {
                instance.storageBank[name.ToString()] = instance.InitializeBlob(Platform.Instance.LoadBlob(name.ToString()));                
            }
        }
#else
		if (instance != null)
        {
            instance.storageBank[name.ToString()] = instance.InitializeBlob(Platform.Instance.LoadBlob(name.ToString()));
        }
#endif
    }

    /// <summary>
    /// Procedure to load collection of panels shattered into separated files for merging purposes
    /// </summary>
    /// <param name="name">name of the blob parenting collection</param>
    /// <returns>true if collection is found not empty</returns>
    static public bool LoadStorageFromCollection(BlobNames name)
    {
#if UNITY_EDITOR
        if (instance != null && Platform.Instance != null)
        {
            string blobName = name.ToString();

            byte[] collectionData = Platform.Instance.LoadBlob(blobName + "_collection");
            if (collectionData == null || collectionData.Length < 2)
            {
                return false;
            }

            BinaryFormatter bformatter = new BinaryFormatter();
            Storage storage = new Storage();
            MemoryStream collectionStream = new MemoryStream(collectionData);
            StreamReader reader = new StreamReader(collectionStream);
            collectionStream.Position = 0;

            string instanceName;

            while (true)
            {
                try
                {
                    instanceName = reader.ReadLine();

                    if (instanceName == null) break;

                    byte[] instanceData = Platform.Instance.LoadBlob("z_" + instanceName);

                    if (instanceData.Length == 0) break;
                 		        
			        MemoryStream ms = new MemoryStream(instanceData);	
			        ms.Position = 0;
                    if (ms.Length != 0)
                    {                        
                        
                        System.Object o = bformatter.Deserialize(ms);
                        if (o is ISerializable)
                        {
                            storage.dictionary.Add(instanceName, (ISerializable)o);
                        }
                        else
                        {
                            break;
                        }
                    }	        
                    else
                    {
                        break;
                    }
		        }
                catch (Exception)
		        {
                    break;
		        }
            }

            if (storage.dictionary.Length() > 0)
            {
                instance.storageBank[blobName] = storage;
                return true;
            }
        }
#endif
        return false;
    }

    /// <summary>
    /// saves storage to external data block for later use
    /// </summary>
    /// <param name="name">name of the blob to be saved</param>    
    /// <returns></returns>
    static public void SaveStorage(BlobNames name)
    {
        SaveStorage(name, false);
    }

    /// <summary>
    /// saves storage to external data block for later use
    /// </summary>
    /// <param name="name">name of the blob to be saved</param>
    /// <param name="saveInCollection">indicates if we will make a collection of data to save along with the main file
    /// <returns></returns>
    static public void SaveStorage(BlobNames name, bool saveInCollection)
    {
#if UNITY_EDITOR
        if (instance != null && Platform.Instance != null)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(ms, GetStorage(name));

            Platform.Instance.StoreBlob(name.ToString(), ms.GetBuffer());
            
            if (saveInCollection)
            {
                SaveStorageAsCollection(name);
            }
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

    /// <summary>
    /// Saves data in collection of files allowing to make merge
    /// </summary>
    /// <param name="name">name of the blob to be saved</param>
    /// <returns></returns>
    static public void SaveStorageAsCollection(BlobNames name)
    {
#if UNITY_EDITOR
        if (instance != null && Platform.Instance != null)
        {            

            string blobName = name.ToString();

            BinaryFormatter bformatter = new BinaryFormatter();
            Storage storage = GetStorage(name);

            if (storage.dictionary.Length() < 1) return;

            MemoryStream blobRecord = new MemoryStream();
            StreamWriter writter = new StreamWriter(blobRecord);

            StorageDictionary dictionary = storage.dictionary.Get(UIManager.UIPannels) as StorageDictionary;

            if (dictionary == null)
            {
                dictionary = storage.dictionary;
            }

            for (int i = 0; i < dictionary.Length(); i++)
            {
                string dataName;
                ISerializable data;
                dictionary.Get(i, out dataName, out data);

                if (dataName == UIManager.UIPannels)
                {
                    continue;
                }

                MemoryStream ms = new MemoryStream();
                bformatter.Serialize(ms, data);
                Platform.Instance.StoreBlob("z_" + dataName, ms.GetBuffer());

                writter.WriteLine(dataName);                
            }
            writter.Flush();
            //write collection record
            Platform.Instance.StoreBlob(blobName + "_collection", blobRecord.GetBuffer());
        }
#endif

    }
}
