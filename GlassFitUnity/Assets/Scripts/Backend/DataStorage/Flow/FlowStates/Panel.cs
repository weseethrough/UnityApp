using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

/// <summary>
/// basic panel which allows to show ui
/// </summary>
[Serializable]
public class Panel : FlowState
{
    static public string[] InteractivePrefabs = { "UIComponents/Button",
												  "MainGUIComponents/ResetGyroButton",
												  "MainGUIComponents/SettingsButton",
												  "SettingsComponent/IndoorButton",
												  "Friend List/ChallengeButton"};    
    public FlowPanelComponent panelNodeData;

    [NonSerialized()] public GameObject physicalWidgetRoot;    

    /// <summary>
    /// default constructor
    /// </summary>
    /// <returns></returns>
	public Panel() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public Panel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
	{
        this.panelNodeData = (FlowPanelComponent)info.GetValue("panelNodeData", typeof(FlowPanelComponent));
        if (this.panelNodeData != null)
        {
            this.panelNodeData.RefreshData();
        }
        else
        {
            int a = 0;
        }
        
    }

    /// <summary>
    /// serialization function called by serializer
    /// </summary>
    /// <param name="info">serialziation info where all data would be pushed to</param>
    /// <param name="ctxt">serialzation context</param>
    /// <returns></returns>
    public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        base.GetObjectData(info, ctxt);
        info.AddValue("panelNodeData", this.panelNodeData);        
   	}

    /// <summary>
    /// Gets display name of the node, helps with node identification in editor
    /// </summary>
    /// <returns>name of the node</returns>
    public override string GetDisplayName()
    {
        base.GetDisplayName();
        
        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return this.GetType().ToString() + ": " + gName.Value;
        }
        return this.GetType().ToString() + ": UnInitialzied";
    }

    /// <summary>
    /// initializes node and creates name for it. Makes as well default iput/output connection sockets
    /// </summary>
    /// <returns></returns>
    protected override void Initialize()
    {
        base.Initialize();

        Size = new Vector2(250, 80);
        NewInput("Enter", "Flow");
       // NewOutput("Exit", "Flow");
        NewParameter("Type", GraphValueType.UIPrefab, ""); 
        NewParameter("Name", GraphValueType.String, "Set Panel Title");
        NewParameter("Settings", GraphValueType.Settings, "");        
    }

    /// <summary>
    /// refreshes connections lists
    /// </summary>
    /// <returns></returns>
    public override void RebuildConnections()
    {
        base.RebuildConnections();

        //Inputs.Clear();
        if (Outputs != null) Outputs.Clear();        

        GParameter gType = Parameters.Find(r => r.Key == "Type");

        SerializedNode node = GetPanelSerializationNode(gType.Value);
        LookForInteractiveItems(node);

        int count = Mathf.Max(Inputs.Count, Outputs.Count);

        UpdateSize();

        RefreshNodeData();
        
    }

    /// <summary>
    /// builds flow panel component data based on serialized nodes based on panel specified name stored as variable "Type" on parameter list
    /// </summary>
    /// <returns></returns>
    public void RefreshNodeData()
    {
        GParameter gType = Parameters.Find(r => r.Key == "Type");

        panelNodeData = new FlowPanelComponent(GetPanelSerializationNode(gType.Value));
    }

    /// <summary>
    /// finds buttons on current panel 
    /// </summary>
    /// <param name="node">root node</param>
    /// <returns></returns>
    private void LookForInteractiveItems(SerializedNode node)
    {
        if (node == null) return;

        foreach (string s in InteractivePrefabs)
        {
            if (node.GetPrefabName() == s)
            {                                
                NewOutput(node.GetName(),"Flow");
            }
        }

        for (int i = 0; i < node.subBranches.Count; i++)
        {
            LookForInteractiveItems(node.subBranches[i]);
        }
    }

    /// <summary>
    /// finds all serializable settings on serializable node
    /// </summary>
    /// <param name="sn">serialziable node source to be analized for serialziable components</param>
    /// <returns></returns>
    private void CreateCustomizationParams(SerializedNode sn)
    {
        SerializableSettings ss = sn != null ? sn.GetSerializableSettings() : null;        
    }

    /// <summary>
    /// finds saved serialized structure and returns root of it
    /// </summary>
    /// <param name="selectedName">lookup name of the screen</param>
    /// <returns>screen serialized root</returns>
    public SerializedNode GetPanelSerializationNode(string selectedName)
    {
        Storage s = DataStore.GetStorage(DataStore.BlobNames.ui_panels);
        if (s == null || s.dictionary == null)
        {           
            return null;
        }

        StorageDictionary screens = Panel.GetPanelDictionary();
        return screens != null ? screens.Get(selectedName) as SerializedNode : null;
    }

    /// <summary>
    /// function called when screen started enter process
    /// </summary>
    /// <returns></returns>
    public override void EnterStart()
    {
        base.EnterStart();

        UIManager script = (UIManager)GameObject.FindObjectOfType(typeof(UIManager));        
        StorageDictionary screensDictionary = Panel.GetPanelDictionary();

        if (script == null)
        {
            Debug.LogError("Scene requires to have UIManager in its root");
        }       
        else if (screensDictionary == null)
        {
            Debug.LogError("Scene requires to have screensDictionary which cant be found");
        }
        else
        {
            GParameter gType = Parameters.Find(r => r.Key == "Type");
            ISerializable data = screensDictionary.Get(gType.Value);
            if (data != null)
            {                
                GParameter gName = Parameters.Find(r => r.Key == "Name");
                physicalWidgetRoot = script.LoadScene((SerializedNode)data, GetWidgetRootName(), panelNodeData);
                if (physicalWidgetRoot != null)
                {
                    physicalWidgetRoot.name = GetWidgetRootName() + "_" + gType.Value + "_" + gName.Value;
                    Debug.Log("Name " + physicalWidgetRoot.name);
                }                
            }

            if (physicalWidgetRoot != null)
            {
                Component[] buttons = physicalWidgetRoot.GetComponentsInChildren(typeof(UIButtonColor));
                foreach (Component b in buttons)
                {
                    UIButtonColor bScript = b as UIButtonColor;
                    FlowButton fb = bScript != null ? bScript.GetComponent<FlowButton>() : null;
                    if (fb == null)
                    {
                        fb = bScript.gameObject.AddComponent<FlowButton>();
                    }

                    fb.owner = this;
                    fb.name = fb.transform.parent.name;
                }
            }
        }
    }

    /// <summary>
    /// exit finalization and clearing process
    /// </summary>
    /// <returns></returns>
    public override void Exited()
    {
        base.Exited();

        UIManager script = (UIManager)GameObject.FindObjectOfType(typeof(UIManager));
        if (physicalWidgetRoot != null)
        {
            GameObject.Destroy(physicalWidgetRoot);        
        }        
    }

    /// <summary>
    /// whenever button get clicked it would be handled here
    /// </summary>
    /// <param name="button">button which send this event</param>
    /// <returns></returns>
    public virtual void OnClick(FlowButton button)
    {
        if (Outputs.Count > 0 && parentMachine != null)
        {
            GConnector gConect = Outputs.Find(r => r.Name == button.name);
            if (gConect != null)
            {
                ConnectionWithCall(gConect, button);                
            }            
        }
        else
        {
            Debug.LogError("Dead end");
        }
    }

    /// <summary>
    /// function which calls exiting function and if it succeed then continues along connector
    /// </summary>
    /// <param name="gConect">connector to follow</param>
    /// <param name="button">button which triggered event</param>
    /// <returns></returns>
    public void ConnectionWithCall(GConnector gConect, FlowButton button)
    {
        if (gConect.EventFunction != null && gConect.EventFunction != "")
        {
            if (CallStaticFunction(gConect.EventFunction, button))
            {
                parentMachine.FollowConnection(gConect);
            }
            else
            {
                Debug.Log("Debug: Function forbids further navigation");
            }
        }
        else
        {
            parentMachine.FollowConnection(gConect);
        }
    }

    /// <summary>
    /// buttons pressed or released on this panel would send events here
    /// </summary>
    /// <param name="button">button which send event</param>
    /// <param name="isDown">is it event on press down ? </param>
    /// <returns></returns>
    public virtual void OnPress(FlowButton button, bool isDown)
    {

    }

    /// <summary>
    /// function allowing to back from the panel to previously visited one
    /// </summary>
    /// <returns></returns>
    public virtual void OnBack()
    {
        parentMachine.FollowBack();
    }

    /// <summary>
    /// function structure which helps with calling static functions from connectors
    /// </summary>
    /// <param name="functionName">function name to be called</param>
    /// <param name="caller">button which have initialzied process</param>
    /// <returns>true is indication that connection should continue</returns>
    public bool CallStaticFunction(string functionName, FlowButton caller)
    {
        MemberInfo[] info = typeof(ButtonFunctionCollection).GetMember(functionName);

        if (info.Length == 1)
        {
            System.Object[] newParams = new System.Object[2];
            newParams[0] = caller;
            newParams[1] = this;
            bool ret = (bool)typeof(ButtonFunctionCollection).InvokeMember(functionName, 
                                    BindingFlags.InvokeMethod | 
                                    BindingFlags.Public |
                                    BindingFlags.Static, 
                                    null, null, newParams);
            return ret;
        }
              
        return true;
    }

    /// <summary>
    /// checks if class have button data and at least one button. As well do the parent checks if screen type is set
    /// </summary>
    /// <returns></returns>
    public override bool IsValid()
    {
        GParameter gType = Parameters.Find(r => r.Key == "Type");

        return base.IsValid() && gType != null && gType.Value != null && gType.Value != "Null";
    }
    

    /// <summary>
    /// Provides name of the widget, by default it is a widget of 2d graphics
    /// </summary>
    /// <returns>const name widget root name</returns>
    public virtual string GetWidgetRootName()
    {
        return "Widgets Container";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static StorageDictionary GetPanelDictionary()
    {
        return GetPanelDictionary(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tryOldOne"></param>
    /// <returns></returns>
    public static StorageDictionary GetPanelDictionary(bool tryOldOne)
    {
        Storage s = DataStore.GetStorage(DataStore.BlobNames.ui_panels);
        StorageDictionary screensDictionary = null;
        if (tryOldOne)
        {
            screensDictionary = (StorageDictionary)s.dictionary.Get(UIManager.UIPannels);
        }


        if (screensDictionary == null)
        {
            screensDictionary = s.dictionary;
        }

        return screensDictionary;
    }
}
