using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[ExecuteInEditMode]
public class DataStorage : MonoBehaviour 
{
    //static accees and properties
	static public DataStorage 	instance;
	
	
    //public properties
	//Note! blob names are used by machine to define file names. Ensure any names used are standard characters for file name.
	public string mainBlobName 	= "core";
	public string localizationBlobName 	= "locEn";

    //private properties
    private Storage mainData;
	private Storage localizationData;
	
	private PlatformDummy platform;
	
	
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("You should have only one datastorage at a time!");
        }
        instance = this;
			
		
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 100), "Initialize"))
        {
            Debug.Log("Data storage initialziation!");
            Initialize(null);
        }
    }

    public void Initialize(byte[] source)
    {
       /* test code */
         
        platform = new PlatformDummy();
		
		MemoryStream ms = new MemoryStream();
		StringSerializer ss = new StringSerializer();
		
		ss.SetString("My Main Blob");			
		ss.WriteToStream(ms);
		int len = (int)ms.Length;
		platform.StoreBlob(mainBlobName, ms.GetBuffer());
		Debug.Log("Stream length "+ms.Length);
		
		MemoryStream ms2 = new MemoryStream();
		
		StringStorageDictionary ssd = new StringStorageDictionary();
		ssd.Add("lng_ok", "OK");
		ssd.Add("lng_cancel", "OK");
		ssd.Add("lng_next", "NEXT");
		ssd.Add("lng_previous", "PREV");
		
		Storage storage = new Storage();
		storage.dictionary.Add("Generic", ssd);
		BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Serialize(ms2, storage);
		
		platform.StoreBlob(localizationBlobName, ms2.GetBuffer());
		
        /*Debug.Log("Stream length "+ms2.Length);
		
        
		
        InitializeBlob( platform.LoadBlob(mainBlobName) );*/
        localizationData = InitializeBlob(platform.LoadBlob(localizationBlobName));
		
		
		
		
	}
	
	private Storage InitializeBlob(byte[] source)
	{
		/* test code */
		
		if (source == null)
		{
			Debug.LogError("Blob doesnt exist")	;
			return null;			
		}
		
		MemoryStream ms = new MemoryStream(source);	
		ms.Position = 0;

        BinaryFormatter bformatter = new BinaryFormatter();
        System.Object o = bformatter.Deserialize(ms);
        Storage storage = (Storage)o;

        return storage;
		
	}

}
