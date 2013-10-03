using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class DataStorage : MonoBehaviour 
{
    //static accees and properties
	static public DataStorage 	instance;
	
    //public properties
	public string mainBlobName 	= "core";

    //private properties
    private Dictionary<String, DataStoredType> data;
	
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("You should have only one datastorage at a time!");
        }
        instance = this;
    }

    public void Initialize(byte[] source)
    {
        
    }

}
