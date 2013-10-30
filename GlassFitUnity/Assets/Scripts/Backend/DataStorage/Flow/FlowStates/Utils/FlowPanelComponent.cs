using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System;

#if UNITY_EDITOR
using UnityEditor; 

#endif

[Serializable]
public class FlowPanelComponent
{   
    public List<SerializableSettings> settings;
    public List<string> names;

    [NonSerialized()] private bool[][] foldout;
    [NonSerialized()] private bool[] foldoutRoot;
    [NonSerialized()] private const int subtreeOffset = 10;

    public FlowPanelComponent(SerializedNode panelNode)
    {        
        RefreshData(panelNode);
    }

    public void OnInspectorGUI(float width)
    {
#if UNITY_EDITOR
        if (names == null) return;

        for (int i=0; i< names.Count; i++)
        {
            GUI.color = Color.white;
            foldoutRoot[i] = EditorGUILayout.Foldout(foldoutRoot[i], names[i]);            
            if (foldoutRoot[i])
            {
                List<SingleComponent> componentList = settings[i].GetComponents();
                if (foldout[i] == null || foldout[i].Length < componentList.Count)
                {
                    foldout[i] = new bool[componentList.Count];
                }
                for (int id =0; id< componentList.Count; id++)
                {
                    //skip serializable component
                    if (componentList[id].name == "UISerializable") continue;
                    
                    GUI.color = Color.yellow;
                    foldout[i][id] = EditorGUILayout.Foldout(foldout[i][id], componentList[id].name);
                    if (foldout[i][id])
                    {
                        
                        if (componentList[id].intData != null)
                        {
                            int data;
                            int data2;
                            string sName;
                            GUI.color = Color.green;
                            for (int k = 0; k < componentList[id].intData.Length(); k++)
                            {
                                componentList[id].intData.Get(k, out sName, out data);
                                EditorGUILayout.BeginHorizontal(GUILayout.Width(width));                                
                                EditorGUILayout.LabelField(sName, GUILayout.Width(width /2));
                                data2 = EditorGUILayout.IntField(data, GUILayout.Width((width / 2) - 5));
                                EditorGUILayout.EndHorizontal();
                                if (data2 != data)
                                {
                                    componentList[id].intData.Set(data2, sName);
                                }
                            }
                        }
                        if (componentList[id].intData != null)
                        {
                            string data;
                            string data2;
                            string sName;
                            for (int k = 0; k < componentList[id].strData.Length(); k++)
                            {
                                componentList[id].strData.Get(k, out sName, out data);
                                EditorGUILayout.BeginHorizontal(GUILayout.Width(width));                                
                                EditorGUILayout.LabelField(sName, GUILayout.Width(width / 2));
                                data2 = EditorGUILayout.TextField(data, GUILayout.Width((width / 2)-5));
                                EditorGUILayout.EndHorizontal();
                                if (data2 != data)
                                {
                                    componentList[id].strData.Set(data2, sName);
                                }
                            }
                        }
                        if (componentList[id].intData != null)
                        {
                            double data;
                            double data2;
                            string sName;
                            for (int k = 0; k < componentList[id].doubleData.Length(); k++)
                            {                                
                                componentList[id].doubleData.Get(k, out sName, out data);
                                EditorGUILayout.BeginHorizontal(GUILayout.Width(width));                                
                                EditorGUILayout.LabelField(sName, GUILayout.Width(width / 2));
                                data2 = EditorGUILayout.FloatField((float)data, GUILayout.Width((width / 2) - 5));
                                EditorGUILayout.EndHorizontal();
                                if (data2 != data)
                                {
                                    componentList[id].doubleData.Set(data2, sName);
                                }
                            }
                        }
                    }
                }                
            }
        }
        GUI.color = Color.white;
#endif
    }    

    public void RefreshData(SerializedNode panelNode)
    {
        if (panelNode == null) return;

        List<SerializedNode> list = LookForCustomizableItems(panelNode);
        settings = new List<SerializableSettings>(list.Count);
        names = new List<string>(list.Count);
        foldout = new bool[list.Count][];
        foldoutRoot = new bool[list.Count];

        for (int i = 0; i < list.Count; i++)
        {
            settings.Add(list[i].GetSerializableSettings().Clone());
            names.Add(list[i].GetName());
            foldoutRoot[i] = false;            
        }
    }

    public void RefreshData()
    {
        if (settings == null) return;

        foldout = new bool[settings.Count][];
        foldoutRoot = new bool[settings.Count];

        for (int i = 0; i < settings.Count; i++)
        {            
            foldoutRoot[i] = false;
        }
    }

    private List<SerializedNode> LookForCustomizableItems(SerializedNode node)
    {
        if (node == null) return null;

        List<SerializedNode> retList = null;

        SerializableSettings ss = node.GetSerializableSettings();
        SingleComponent sc = ss != null ? ss.GetComponent("UISerializable") : null;

        //booleans are stored as integers which decreases number of list types we need to have
        int exposed = sc != null ? sc.GetInitializedIntDict().Get("exposeInFlow") : 0;
        if (exposed != 0)
        {
            if (retList == null)
            {
                retList = new List<SerializedNode>();
            }
            retList.Add(node);
        }


        for (int i = 0; i < node.subBranches.Count; i++)
        {
            List<SerializedNode> l = LookForCustomizableItems(node.subBranches[i]);
            if (l != null)
            {
                if (retList == null)
                {
                    retList = new List<SerializedNode>();
                }
                retList.AddRange(l);
            }
        }
        return retList;
    }


}
