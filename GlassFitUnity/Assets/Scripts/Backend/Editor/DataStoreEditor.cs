using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Simple storage editor allowing to clear current blobs
/// </summary>
[CustomEditor(typeof(DataStore))]
public class DataStoreEditor : Editor
{

    /// <summary>
    /// draws button which allows to remove stored externally blobs
    /// </summary>
    /// <returns></returns>
    public override void OnInspectorGUI()
    {
        DataStore ds = target as DataStore;
        int max = (int)DataStore.BlobNames.maxItem;

        if (GUILayout.Button("Remove Blobs"))
        {
            for (int i = 0; i < max; i++)
            {
                string name = ((DataStore.BlobNames)(i)).ToString();                
                PlatformDummy.Instance.EraseBlob(name);                
            }
        }
    }
}