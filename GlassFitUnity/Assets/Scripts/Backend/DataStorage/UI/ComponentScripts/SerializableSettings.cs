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
    private List<SingleComponent> components;

    public SerializableSettings()
    {
        this.components = new List<SingleComponent>();
    }

    public SerializableSettings(GameObject go)
    {
        ReadSettings(go);
        ReadGameObjectComponents(go);
    }

    public SerializableSettings(SerializationInfo info, StreamingContext ctxt)
	{
        this.components = (List<SingleComponent>)info.GetValue("Components", typeof(List<SingleComponent>));        
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        info.AddValue("Components", this.components);        
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
                SingleComponent sc = new SingleComponent();
                sc.name = myType.ToString();

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
