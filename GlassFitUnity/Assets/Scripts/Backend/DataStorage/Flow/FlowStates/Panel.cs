using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

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
    [NonSerialized()] private string widgetRootName = "Widgets Container";
	
	public Panel() : base() {}
	
    public Panel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
	{
        this.panelNodeData = (FlowPanelComponent)info.GetValue("panelNodeData", typeof(FlowPanelComponent));
        this.panelNodeData.RefreshData();
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        base.GetObjectData(info, ctxt);
        info.AddValue("panelNodeData", this.panelNodeData);        
   	}

    public override string GetDisplayName()
    {
        base.GetDisplayName();
        
        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return "Panel: " + gName.Value;
        }
        return "Panel: UnInitialzied";
    }

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

    public override void RebuildConnections()
    {
        base.RebuildConnections();

        //Inputs.Clear();
        if (Outputs != null) Outputs.Clear();        

        GParameter gType = Parameters.Find(r => r.Key == "Type");

        SerializedNode node = GetUIPanelNames(gType.Value);
        LookForInteractiveItems(node);

        int count = Mathf.Max(Inputs.Count, Outputs.Count);

        Size.y = Mathf.Max(count * 25, 80) ;

        RefreshNodeData();
        
    }

    public void RefreshNodeData()
    {
        GParameter gType = Parameters.Find(r => r.Key == "Type");

        panelNodeData = new FlowPanelComponent(GetUIPanelNames(gType.Value));
    }

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

    private void CreateCustomizationParams(SerializedNode sn)
    {
        SerializableSettings ss = sn != null ? sn.GetSerializableSettings() : null;
        
       // NewParameter("Name", GraphValueType.String, "Set Panel Title");
    }

    public SerializedNode GetUIPanelNames(string selectedName)
    {
        Storage s = DataStore.GetStorage(DataStore.BlobNames.ui_panels);
        if (s == null || s.dictionary == null)
        {           
            return null;
        }

        StorageDictionary screens = (StorageDictionary)s.dictionary.Get(UIManager.UIPannels);
        return screens != null ? screens.Get(selectedName) as SerializedNode : null;
    }

    public override void EnterStart()
    {
        base.EnterStart();

        UIManager script = (UIManager)GameObject.FindObjectOfType(typeof(UIManager));
        Storage s = DataStore.GetStorage(DataStore.BlobNames.ui_panels);
        StorageDictionary screensDictionary = s != null ? (StorageDictionary)s.dictionary.Get(UIManager.UIPannels) : null;

        if (script == null)
        {
            Debug.LogError("Scene requires to have UIManager in its root");
        }
        else if (s == null)
        {
            Debug.LogError("Scene requires to have storage 'core' which cant be found");
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
                physicalWidgetRoot = script.LoadScene((SerializedNode)data, widgetRootName, panelNodeData);
                if (physicalWidgetRoot != null)
                {
                    physicalWidgetRoot.name = widgetRootName + "_" + gType.Value + "_" + gName.Value;
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

    public override void Exited()
    {
        base.Exited();

        UIManager script = (UIManager)GameObject.FindObjectOfType(typeof(UIManager));
        if (physicalWidgetRoot != null)
        {
            GameObject.Destroy(physicalWidgetRoot);        
        }        
    }    

    public virtual void OnClick(FlowButton button)
    {
        if (Outputs.Count > 0 && parentMachine != null)
        {
            GConnector gConect = Outputs.Find(r => r.Name == button.name);
            if (gConect != null)
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
        }
        else
        {
            Debug.LogError("Dead end");
        }
    }

    public virtual void OnPress(FlowButton button, bool isDown)
    {

    }

    public bool CallStaticFunction(string functionName, FlowButton caller)
    {
        MemberInfo[] info = typeof(ButtonFunctionCollection).GetMember(functionName);

        if (info.Length == 1)
        {
            System.Object[] newParams = new System.Object[1];
            newParams[0] = caller;
            bool ret = (bool)typeof(ButtonFunctionCollection).InvokeMember(functionName, 
                                    BindingFlags.InvokeMethod | 
                                    BindingFlags.Public |
                                    BindingFlags.Static, 
                                    null, null, newParams);
            return ret;
        }
              
        return true;
    }


    public override bool IsValid()
    {
        GParameter gType = Parameters.Find(r => r.Key == "Type");

        return base.IsValid() && gType != null && gType.Value != null && gType.Value != "Null";
    }
}
