using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(DataStore))]
public class DataStoreEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DataStore ds = target as DataStore;
        int max = (int)DataStore.BlobNames.maxItem;

        if (GUILayout.Button("Remove Blobs"))
        {
            for (int i = 0; i < max; i++)
            {
                string name = ((DataStore.BlobNames)(i)).ToString();                
                ds.platform.EraseBlob(name);                
            }
        }
    }
}