using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor; 
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
        UISerializable script = go.GetComponent<UISerializable>();
        if (script != null)
        {
            this.hasUISerializable      = true;
            this.keepAliveIfPossible    = script.keepAliveIfPossible;
        }
        else
        {
            this.hasUISerializable = false;
        }

        UIComponentSettings settings = go.GetComponent<UIComponentSettings>();
        if (settings != null)
        {
            this.hasUIComponentSettings = true;
            this.textLabel = settings.textLabel;
        }
        else
        {
            this.hasUIComponentSettings = false;
        }
    }

    public void LoadSettingsTo(GameObject go)
    {
        UISerializable script = go.GetComponent<UISerializable>();
        if (script == null && this.hasUISerializable)
        {
            script = go.AddComponent<UISerializable>();
        }

        if (script != null)
        {
            script.keepAliveIfPossible = this.keepAliveIfPossible;
        }

        UIComponentSettings settings = go.GetComponent<UIComponentSettings>();
        if (settings == null && this.hasUIComponentSettings)
        {
            settings = go.AddComponent<UIComponentSettings>();
        }

        if (settings != null)
        {
            settings.textLabel = this.textLabel;
        }
    }
	
}
