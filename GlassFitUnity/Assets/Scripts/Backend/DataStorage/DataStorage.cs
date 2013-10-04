using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class DataStorage : MonoBehaviour 
{
    //static accees and properties
	static public DataStorage 	instance;
	
	
    //public properties
	//Note! blob names are used by machine to define file names. Ensure any names used are standard characters for file name.
	public string mainBlobName 	= "core";
	public string localizationBlobName 	= "locEn";

    //private properties
    private Dictionary<String, DataStoredType> data;
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
        platform = new Platform();
		
		StringSerializer ss = new StringSerializer("My Main Blob");
		platform.StoreBlob(mainBlobName, ss.GetSource());

		StringSerializer ss2 = new StringSerializer("My Localization Blob");
		platform.StoreBlob(localizationBlobName, ss2.GetSource());
		
		InitializeBlob( platform.LoadBlob(mainBlobName) );
		InitializeBlob( platform.LoadBlob(localizationBlobName) );
		
		
	}
	
	private void InitializeBlob(byte[] source)
	{
		if (source == null)
		{
			Debug.LogError("Blob doesnt exist")	;
			return;			
		}
		
		StringSerializer ss = new StringSerializer(source, 0);
		Debug.Log("Loaded string:" + ss.GetString());
		
	}

}
