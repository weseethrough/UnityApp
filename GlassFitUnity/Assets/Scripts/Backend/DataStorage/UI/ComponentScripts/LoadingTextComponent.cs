using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Simple component to control visibility of the loading text gameobject it is attached to
/// </summary>
public class LoadingTextComponent : MonoBehaviour 
{
    private static LoadingTextComponent instance;
    
    void Awake()
    {
        instance = this;
        SetVisibility(true);
    }

    void OnDestroy()
    {
        instance = null;
    }
	
	public static bool IsVisible() {
		if(instance != null) {
			return instance.gameObject.activeSelf;
		} else { 
			//UnityEngine.Debug.Log("LoadingText: object null, returning false");
			return false;
		}
	}

    public static void SetVisibility(bool visible)
    {
        if (instance != null)
        {
            instance.gameObject.SetActive(visible);
        } else {
			//UnityEngine.Debug.Log("LoadingText: object is null!");
		}
    }
}
