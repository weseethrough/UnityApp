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
public class MobilePanel : Panel
{

    protected List<ListButtonData> buttonData = new List<ListButtonData>();
    private GestureHelper.OnBack backHandler = null;

    /// <summary>
    /// default constructor
    /// </summary>
    /// <returns></returns>
	public MobilePanel() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public MobilePanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
	{    
        
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
            return "MobilePanel: " + gName.Value;
        }
        return "MobilePanel: UnInitialzied";
    }
    
    public List<ListButtonData> GetButtonData()
    {
        return buttonData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override void Entered()
    {
        base.Entered();

        backHandler = new GestureHelper.OnBack(() =>
        {
            OnBack();
        });
        GestureHelper.onBack += backHandler;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override void ExitStart()
    {
        base.ExitStart();

        GestureHelper.onBack -= backHandler;
        buttonData.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected void AddBackButtonData()
    {
        AddButtonData("buttonBack", "Back", "FollowBack");        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buttonName"></param>
    /// <param name="title"></param>
    /// <param name="function"></param>    
    /// <returns></returns>
    protected void AddButtonData(string buttonName, string title, string function)
    {
        AddButtonData(buttonName, title, function, ListButtonData.ButtonFormat.ButtonPrototype, null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buttonName"></param>
    /// <param name="title"></param>
    /// <param name="function"></param>
    /// <param name="buttonFormat"></param>
    /// <returns></returns>
    protected void AddButtonData(string buttonName, string title, string function, ListButtonData.ButtonFormat buttonFormat)
    {
        AddButtonData(buttonName, title, function, buttonFormat, null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buttonName"></param>
    /// <param name="title"></param>
    /// <param name="function"></param>
    /// <param name="buttonFormat"></param>
    /// <param name="cloneFirstLinkage"></param>
    /// <returns></returns>
    protected void AddButtonData(string buttonName, string title, string function, ListButtonData.ButtonFormat buttonFormat, GConnector cloneFirstLinkage)
    {       
        ListButtonData data = new ListButtonData();
        data.textNormal = title;
        data.buttonName = buttonName;
        data.connectionFunction = function;
        data.buttonFormat = buttonFormat.ToString();

        buttonData.Add(data);

        GConnector gc = NewOutput(buttonName, "Flow");
        gc.EventFunction = function;
        gc.Name = buttonName;
        if (GraphComponent.GetInstance() != null && cloneFirstLinkage != null)
        {
            if (cloneFirstLinkage.Link.Count > 0)
            {
                GraphComponent.GetInstance().Data.Connect(gc, cloneFirstLinkage.Link[0]);
            }
            gc.EventFunction = cloneFirstLinkage.EventFunction;
        }        
    }   

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected GConnector GetBaseButtonConnection()
    {
         GConnector gc = Outputs.Find(r => r.Name == "buttonHandler");
        if (gc == null)
        {
            gc = Outputs.Find(r => r.Name == ListButtonData.ButtonFormat.ButtonPrototype.ToString());
        }


        return gc;
    }
}
