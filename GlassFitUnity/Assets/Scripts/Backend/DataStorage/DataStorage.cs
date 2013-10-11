using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[ExecuteInEditMode]
public class DataStorage : MonoBehaviour 
{    
	static public DataStorage 	instance;
		
    //public properties
	//Note! blob names are used by machine to define file names. Ensure any names used are standard characters for file name.
	public string mainBlobName 	= "core";
	public string localizationBlobName 	= "locEn";

    //private properties
    private Storage coreData;
	private Storage localizationData;
	
	private PlatformDummy platform;
	
	
    void Awake()
    {        
        instance = this;

        if (platform != null)
        {
            Debug.LogWarning("Reinitialziation of datastorage canceled");
            return;
        }
        platform = new PlatformDummy();

        Initialize();
    }   

    public void Initialize()
    {
        coreData  			= InitializeBlob( platform.LoadBlob(mainBlobName) );
        localizationData 	= InitializeBlob( platform.LoadBlob(localizationBlobName) );	
	}
	
	private Storage InitializeBlob(byte[] source)
	{
		/* test code */
		
		if (source == null)
		{
			Debug.LogWarning("Blob doesnt exist, we are creating one");
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
	
	static public Storage GetCoreStorage()
	{
		if (instance != null)
		{
			return instance.coreData;	
		}
		
		return null;
	}
	
	static public Storage GetLocalizationStorage()
	{
		if (instance != null)
		{
			return instance.localizationData;
		}
		
		return null;
	}

    static public void SaveCoreStorage()
    {
        if (instance != null && instance.platform != null)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(ms, instance.coreData);

            instance.platform.StoreBlob(instance.mainBlobName, ms.GetBuffer());
        }
    }
	
}
