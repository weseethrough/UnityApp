using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System;
using System.Reflection;

[Serializable]
public class SerializableSettings : ISerializable 
{
    private List<SingleComponent> components;    

    public SerializableSettings()
    {
        this.components = new List<SingleComponent>();
    }

    public SerializableSettings(GameObject go)
    {        
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

    public void LoadSettingsTo(GameObject go)
    {
        var bindingFlags = BindingFlags.Instance |
                           BindingFlags.Public |
                           BindingFlags.FlattenHierarchy;

        foreach (SingleComponent sc in components)
        {
            Component c = go.GetComponent(sc.name);
            if (c == null)
            {
                c = go.AddComponent(sc.name);
            }

            FieldInfo[] fields = c.GetType().GetFields(bindingFlags);

            foreach (FieldInfo field in fields)
            {
                string fname = field.Name;                
                if (sc.strData != null && field.FieldType == typeof(string))
                {
                    field.SetValue(c,sc.strData.Get(fname));
                }
                else if (sc.intData != null && field.FieldType == typeof(int))
                {
                    field.SetValue(c, sc.intData.Get(fname));
                }
                else if (sc.intData != null && field.FieldType == typeof(bool))
                {
                    field.SetValue(c, sc.intData.Get(fname) == 0 ? false : true);
                }
                else if (sc.doubleData != null && field.FieldType == typeof(float))
                {
                    field.SetValue(c, sc.doubleData.Get(fname));
                }
                else if (sc.doubleData != null && field.FieldType == typeof(double))
                {
                    field.SetValue(c, sc.doubleData.Get(fname));
                }
            }

            UIComponentSettings cs = c as UIComponentSettings;
            if (cs != null)
            {
                cs.Apply();
            }

        }
    }

    private void ReadGameObjectComponents(GameObject go)
    {
        this.components = new List<SingleComponent>();

        if (go.GetComponent<UISerializable>() == null) return;

        Component[] attachedComponents = go.GetComponents<Component>();
        
        var bindingFlags = BindingFlags.Instance |                   
                           BindingFlags.Public |
                           BindingFlags.FlattenHierarchy;

        for (int i=0; i<attachedComponents.Length; i++)
        {            
            System.Type myType = attachedComponents[i].GetType();
            if (myType != typeof(Transform))
            {
                try
                {
                    SingleComponent sc = new SingleComponent();
                    sc.name = myType.ToString();

                    FieldInfo[] fields = attachedComponents[i].GetType().GetFields(bindingFlags);
                    Debug.Log("Displaying the values of the fields of " + myType.ToString() + "(" + fields.Length + ")");

                    foreach (FieldInfo field in fields)
                    {
                        string fname = field.Name;
                        System.Object fValue = field.GetValue(attachedComponents[i]);

                        if (field.FieldType == typeof(string))
                        {
                            sc.GetInitializedStrDict().Add(fname, fValue as string);
                        }
                        else if (field.FieldType == typeof(int))
                        {
                            sc.GetInitializedIntDict().Add(fname, (int)fValue);
                        }
                        else if (field.FieldType == typeof(bool))
                        {
                            sc.GetInitializedIntDict().Add(fname, (bool)fValue == false ? 0 : 1);
                        }
                        else if (field.FieldType == typeof(float))
                        {
                            sc.GetInitializedFloatDict().Add(fname, (float)fValue);
                        }
                        else if (field.FieldType == typeof(double))
                        {
                            sc.GetInitializedFloatDict().Add(fname, (double)fValue);
                        }
                    }

                    components.Add(sc);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }                
    }

    public SingleComponent GetComponent(string name)
    {
        return components.Find(r => r.name == name);
    }

    public List<SingleComponent> GetComponents()
    {
        return components;
    }

    public SerializableSettings Clone()
    {
        SerializableSettings ss = new SerializableSettings();
        for(int i=0; i < components.Count; i++)
        {
            ss.components.Add( components[i].Clone());
        }
        return ss;
    }
}
