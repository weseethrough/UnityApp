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
        SetVisibility(false);
    }

    void OnDestroy()
    {
        instance = null;
    }

    public static void SetVisibility(bool visible)
    {
        if (instance != null)
        {
            instance.gameObject.SetActive(visible);
        }
    }
}
