using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(DataVault))]
public class DataVaultEditor : Editor
{
    enum Type
    {
        STRING,
        INT,
        BOOL, 
        FLOAT,        
    }

    private List<string>    toRemove            = new List<string>();
    private string          newVariableName     = string.Empty;
    private string          newVariableValue    = string.Empty;
    private Type            newVariableType     = Type.STRING;
    
    public override void OnInspectorGUI()
    {
        if (DataVault.data == null)
        {
            DataVault.Initialize();            
        }
        int basicLabelWidth = 100;
        int basicValueWidth = 200;

        foreach (var pair in DataVault.data)
        {
            DataEntry de = (DataEntry)pair.Value;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(pair.Key, GUILayout.Width(basicLabelWidth)); //label
            if (de.storedValue.GetType() == typeof(int))
            {                                
                int oldValue = Convert.ToInt32(de.storedValue);
                int newValue = EditorGUILayout.IntField(oldValue, GUILayout.Width(basicValueWidth)); //field
                if (oldValue != newValue)
                {
                    DataVault.Set(pair.Key, newValue);
                }                                    
            }
            else if (de.storedValue.GetType() == typeof(bool))
            {                
                bool oldValue = Convert.ToBoolean(de.storedValue);
                bool newValue = EditorGUILayout.Toggle(oldValue, GUILayout.Width(basicValueWidth)); //field
                if (oldValue != newValue)
                {
                    DataVault.Set(pair.Key, newValue);
                }
            }
            else if (de.storedValue.GetType() == typeof(string))
            {             
                string oldValue = Convert.ToString(de.storedValue);
                string newValue = EditorGUILayout.TextField(oldValue, GUILayout.Width(basicValueWidth)); //field
                if (oldValue != newValue)
                {
                    DataVault.Set(pair.Key, newValue);
                }
            }
            else if (de.storedValue.GetType() == typeof(float))
            {                
                float oldValue = (float)Convert.ToDouble(de.storedValue);
                float newValue = EditorGUILayout.FloatField(oldValue, GUILayout.Width(basicValueWidth)); //field
                if (oldValue != newValue)
                {
                    DataVault.Set(pair.Key, newValue);
                }
            }
            else 
            {
                EditorGUILayout.LabelField(pair.Value.ToString(), GUILayout.Width(basicValueWidth)); //field
            }

            EditorGUILayout.LabelField("persistence:", GUILayout.Width(80));
            bool oldPersistence = de.persistent;
            bool newPersistence = EditorGUILayout.Toggle(oldPersistence, GUILayout.Width(20)); //persistence
            if (oldPersistence != newPersistence)
            {
                DataVault.SetPersistency(pair.Key, newPersistence);
            }

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                toRemove.Add(pair.Key);
            }
            EditorGUILayout.EndHorizontal();
        }

        foreach (string s in toRemove)
        {
            DataVault.Remove(s);
        }
        toRemove.Clear();        
        
        EditorGUILayout.BeginHorizontal();
            GUILayout.Label("New Value Name:", GUILayout.Width(100));
            newVariableName = EditorGUILayout.TextField( newVariableName, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Value:", GUILayout.Width(100));
            newVariableValue = EditorGUILayout.TextField(newVariableValue, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Type:", GUILayout.Width(100));
            newVariableType = (Type)EditorGUILayout.EnumPopup(newVariableType, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        if (newVariableName.Length == 0 || DataVault.data.ContainsKey(newVariableName))
        {
            GUI.enabled = false;
        }

        if (GUILayout.Button("Add new entry", GUILayout.Width(300)))
        {                       
            switch (newVariableType)
            {
                case Type.STRING:
                    DataVault.Set(newVariableName, newVariableValue);
                    break;
                case Type.INT:
                    DataVault.Set(newVariableName, Convert.ToInt32(newVariableValue));
                    break;
                case Type.BOOL:
                    DataVault.Set(newVariableName, Convert.ToBoolean(newVariableValue));
                    break;
                case Type.FLOAT:
                    DataVault.Set(newVariableName, (float)Convert.ToDouble(newVariableValue));
                    break;
            }

            DataVault.SetPersistency(newVariableName, true);
        }
         
        GUI.enabled = true;
        GUI.color = Color.green;
        if (GUILayout.Button("Save Changes", GUILayout.Width(300)))
        {
            DataVault.SaveToBlob();
        }
        GUI.color = Color.white;
    }

}