using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor; 
using System.Reflection;
#endif


[System.Serializable]
public class SerializableSettings : ISerializable 
{
    public bool     hasUISerializable;
    public bool     hasUIComponentSettings;

    public bool     keepAliveIfPossible;    
    public string   textLabel;
    

    public SerializableSettings()
    {
        this.hasUISerializable      = false;
        this.hasUIComponentSettings = false;
        this.keepAliveIfPossible    = false;
        this.textLabel              = string.Empty;        
    }

    public SerializableSettings(GameObject go)
    {
        ReadSettings(go);
        ReadGameObjectComponents(go);
    }

    public SerializableSettings(SerializationInfo info, StreamingContext ctxt)
	{
        this.hasUISerializable      = (bool)info.GetValue("HasUISerializable", typeof(bool));
        this.hasUIComponentSettings = (bool)info.GetValue("HasUIComponentSettings", typeof(bool));
        this.keepAliveIfPossible    = (bool)info.GetValue("KeepAlive", typeof(bool));
        this.textLabel              = (string)info.GetValue("Label", typeof(string));                
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        info.AddValue("HasUISerializable", this.hasUISerializable);
        info.AddValue("HasUIComponentSettings", this.hasUIComponentSettings);
        info.AddValue("KeepAlive", this.keepAliveIfPossible);
        info.AddValue("Label", this.textLabel);
   	}

    public void ReadSettings(GameObject go)
    {
        /*UISerializable script = go.GetComponent<UISerializable>();
        if (script != null)
        {
            this.hasUISerializable = true;
        }
        else
        {
            this.hasUISerializable = false;
        }

        UIComponentSettings settings = go.GetComponent<UIComponentSettings>();
        if (settings != null)
        {
            this.hasUIComponentSettings = true;
            this.textLabel = settings;
        }
        else
        {
            this.hasUIComponentSettings = false;
        }*/
    }

    public void LoadSettingsTo(GameObject go)
    {
     /*   UISerializable script = go.GetComponent<UISerializable>();
        if (script == null && this.hasUISerializable)
        {
            script = go.AddComponent<UISerializable>();
        }
        
        UIComponentSettings settings = go.GetComponent<UIComponentSettings>();
        if (settings == null && this.hasUIComponentSettings)
        {
            settings = go.AddComponent<UIComponentSettings>();
        }

        if (settings != null)
        {
            settings.textLabel = this.textLabel;
        }*/
    }

    private void ReadGameObjectComponents(GameObject go)
    {
        Component[] componennts = go.GetComponents<Component>();
        
        var bindingFlags = BindingFlags.Instance |                   
                           BindingFlags.Public |
                           BindingFlags.FlattenHierarchy;

        for (int i=0; i<componennts.Length; i++)
        {
            System.Type myType = componennts[i].GetType();
            try
            {                                               
                FieldInfo[] fields = componennts[i].GetType().GetFields(bindingFlags);
                Debug.Log("Displaying the values of the fields of "+myType.ToString() + "("+ fields.Length+")");
                
                foreach (FieldInfo field in fields)
                {
                    string fname            = field.Name;
                    System.Object fValue    = field.GetValue(componennts[i]);

                    Debug.Log("Field " + fname + "(" + fValue.ToString() + ")");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Exception : " + e.Message);
            }
        }                
    }
	
}
