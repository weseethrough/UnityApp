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
public class SingleComponent : ISerializable 
{
    public IntStorageDictionary         intData;
    public FloatStorageDictionary       floatData;
    public StringStorageDictionary      strData;
    public string                       name;
    
    public SingleComponent()
    {       
    }    

    public SingleComponent(SerializationInfo info, StreamingContext ctxt)
	{
        this.intData                = (IntStorageDictionary)info.GetValue("IntData", typeof(IntStorageDictionary));
        this.floatData              = (FloatStorageDictionary)info.GetValue("FloatData", typeof(FloatStorageDictionary));
        this.strData                = (StringStorageDictionary)info.GetValue("StrData", typeof(StringStorageDictionary));
        this.name                   = (string)info.GetValue("Name", typeof(string));                
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        info.AddValue("IntData", this.intData);
        info.AddValue("FloatData", this.floatData);
        info.AddValue("StrData", this.strData);
        info.AddValue("Name", this.name);
   	}

    public IntStorageDictionary GetInitializedIntDict()
    {
        if (intData == null) intData = new IntStorageDictionary();
        return intData;
    }

    public FloatStorageDictionary GetInitializedFloatDict()
    {
        if (floatData == null) floatData = new FloatStorageDictionary();
        return floatData;
    }

    public StringStorageDictionary GetInitializedStrDict()
    {
        if (strData == null) strData = new StringStorageDictionary();
        return strData;
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
