using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System;
using System.Reflection;

/// <summary>
/// Class collecting data about game object building serializable structure of its behaviors
/// </summary>
[Serializable]
public class SerializableSettingsBase : ISerializable 
{
    protected List<SingleComponentBase> components;    

    /// <summary>
    /// default constructor initialization
    /// </summary>
    /// <returns></returns>
    public SerializableSettingsBase()
    {
        this.components = new List<SingleComponentBase>();
    }

    /// <summary>
    /// gameobject based constructor which reads its structure and prepares for serialization
    /// </summary>
    /// <param name="go">gameobject containing components which can be processed by serialziator(simle clasess and basic type variables only, no pointers!)</param>
    /// <returns></returns>
    public SerializableSettingsBase(GameObject go)
    {        
        ReadGameObjectComponents(go);
    }

    /// <summary>
    /// deserialization constructor
    /// </summary>
    /// <param name="info">serialization info containing parameters details</param>
    /// <param name="ctxt">serialziation context</param>
    /// <returns></returns>
    public SerializableSettingsBase(SerializationInfo info, StreamingContext ctxt)
	{
       // this.components = (List<SingleComponentBase>)info.GetValue("Components", typeof(List<SingleComponentBase>));        
	}
	
	/// <summary>
	/// serialization functionality called by serializator
	/// </summary>
    /// <param name="info">serialization info containing parameters details</param>
    /// <param name="ctxt">serialziation context</param>
	/// <returns></returns>
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        info.AddValue("Components", this.components);        
   	}

    /// <summary>
    /// rebuild data to gameobject attaching missing components and configuring them according to serialziezd data
    /// </summary>
    /// <param name="go">target gameobject which should contain components or would get them as a reconstruction process. Prefab root</param>
    /// <returns></returns>
    public void LoadSettingsTo(GameObject go)
    {
        var bindingFlags = BindingFlags.Instance |
                           BindingFlags.Public |
                           BindingFlags.FlattenHierarchy;

        foreach (SingleComponentBase sc in components)
        {
            Component c = go.GetComponent(sc.name);
            if (c == null)
            {
                break;
                //c = go.AddComponent(sc.name);
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
                cs.Register();                
            }

        }
    }

    /// <summary>
    /// reads and saves gameobejct components and variables
    /// </summary>
    /// <param name="go">target gameobject which is prepared for serialziation</param>
    /// <returns></returns>
    private void ReadGameObjectComponents(GameObject go)
    {
        this.components = new List<SingleComponentBase>();

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
                    SingleComponentBase sc = new SingleComponentBase();
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

    /// <summary>
    /// searches component by name in stored list, returns 
    /// </summary>
    /// <param name="name">name of the component in question</param>
    /// <returns>serialization-ready structure requested usually for rebuild</returns>
    public SingleComponentBase GetComponent(string name)
    {
        return components.Find(r => r.name == name);
    }

    /// <summary>
    /// get all single components related to creator's game object
    /// </summary>
    /// <returns>list of all single components created during preparation for serialization</returns>
    public List<SingleComponentBase> GetComponents()
    {
        return components;
    }

    /// <summary>
    /// makes copy of the serializable setting and all its stored components
    /// </summary>
    /// <returns>copy of the serializable setting and all its stored components</returns>
    public SerializableSettingsBase Clone()
    {
        SerializableSettingsBase ss = new SerializableSettingsBase();
        for(int i=0; i < components.Count; i++)
        {
            ss.components.Add( components[i].Clone());
        }
        return ss;
    }
}
