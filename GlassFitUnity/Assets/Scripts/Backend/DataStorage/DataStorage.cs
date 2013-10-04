using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

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
	
	private Platform platform;
	
	
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
            print("You clicked the button!");
            Initialize(null);
        }
    }

    public void Initialize(byte[] source)
    {
       /* test code
        * 
        * platform = new Platform();
		
		MemoryStream ms = new MemoryStream();
		StringSerializer ss = new StringSerializer();
		
		ss.SetString("My Main Blob");			
		ss.WriteToStream(ms);
		int len = (int)ms.Length;
		platform.StoreBlob(mainBlobName, ms.GetBuffer());
		Debug.Log("Stream length "+ms.Length);
		
		MemoryStream ms2 = new MemoryStream();
		ss.SetString("My Localization Blob");
		ss.WriteToStream(ms2);
		Debug.Log("Stream length "+ms2.Length);
		
		platform.StoreBlob(localizationBlobName, ms2.GetBuffer());
		
		InitializeBlob( platform.LoadBlob(mainBlobName) );
		InitializeBlob( platform.LoadBlob(localizationBlobName) );*/
		
		
	}
	
	private void InitializeBlob(byte[] source)
	{
		/*test code
		 * if (source == null)
		{
			Debug.LogError("Blob doesnt exist")	;
			return;			
		}
		
		MemoryStream ms = new MemoryStream(source);	
		ms.Position = 0;
		StringSerializer ss = new StringSerializer(ms);
		
		Debug.Log("Loaded string:" + ss.GetString());*/
		
	}

}
