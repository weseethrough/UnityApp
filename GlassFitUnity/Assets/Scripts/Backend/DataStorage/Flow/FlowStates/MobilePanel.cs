using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

using RaceYourself.Models;

/// <summary>
/// basic panel which allows to show ui
/// </summary>
[Serializable]
public class MobilePanel : Panel
{

	protected Dictionary<string, List<ListButtonData>> buttonDataMap = new Dictionary<string, List<ListButtonData>>();
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
		return GetButtonData("default");
	}

    public List<ListButtonData> GetButtonData(string list)
    {
		if (!buttonDataMap.ContainsKey(list)) 
		{
			lock(buttonDataMap) {
				if (!buttonDataMap.ContainsKey(list)) {
					buttonDataMap[list] = new List<ListButtonData>();
				}
			}
		}
		return buttonDataMap[list];
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
        buttonDataMap.Clear();
    }

	protected void AddButtonData(string buttonName, Dictionary<string, string> buttonDictionary, string function)
	{
		AddButtonData("default", buttonName, buttonDictionary, function, null, ListButtonData.ButtonFormat.ButtonPrototype, null);
	}
	
	protected void AddButtonData(string buttonName, Dictionary<string, string> buttonDictionary, string function, ListButtonData.ButtonFormat buttonFormat)
	{
		AddButtonData("default", buttonName, buttonDictionary, function, null, buttonFormat, null);
	}
	
	protected void AddButtonData(string buttonName, Dictionary<string, string> buttonDictionary, string function, ListButtonData.ButtonFormat buttonFormat, GConnector cloneFirstLinkage)
	{       
		AddButtonData("default", buttonName, buttonDictionary, function, null, buttonFormat, cloneFirstLinkage);
	}   
	
	protected void AddButtonData(string buttonName, Dictionary<string, string> buttonDictionary, string function, Dictionary<string, Dictionary<string, string>> imageDictionary, ListButtonData.ButtonFormat buttonFormat, GConnector cloneFirstLinkage)
	{
		AddButtonData("default", buttonName, buttonDictionary, function, null, buttonFormat, cloneFirstLinkage);
	}

	/// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
	protected void AddBackButtonData(string list)
    {
//		Dictionary<string, string> dictionary = 
//        AddButtonData("buttonBack", "Back", "FollowBack");        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buttonName"></param>
    /// <param name="title"></param>
    /// <param name="function"></param>    
    /// <returns></returns>
	protected void AddButtonData(string list, string buttonName, Dictionary<string, string> buttonDictionary, string function)
    {
		AddButtonData(list, buttonName, buttonDictionary, function, null, ListButtonData.ButtonFormat.ButtonPrototype, null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buttonName"></param>
    /// <param name="title"></param>
    /// <param name="function"></param>
    /// <param name="buttonFormat"></param>
    /// <returns></returns>
	protected void AddButtonData(string list, string buttonName, Dictionary<string, string> buttonDictionary, string function, ListButtonData.ButtonFormat buttonFormat)
    {
        AddButtonData(list, buttonName, buttonDictionary, function, null, buttonFormat, null);
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
	protected void AddButtonData(string list, string buttonName, Dictionary<string, string> buttonDictionary, string function, ListButtonData.ButtonFormat buttonFormat, GConnector cloneFirstLinkage)
    {       
		AddButtonData(list, buttonName, buttonDictionary, function, null, buttonFormat, cloneFirstLinkage);
	}   
	
    protected void AddButtonData(string list, string buttonName, Dictionary<string, string> buttonDictionary, string function, Dictionary<string, Dictionary<string, string>> imageDictionary, ListButtonData.ButtonFormat buttonFormat, GConnector cloneFirstLinkage)
    {
        AddButtonData(list, buttonName, buttonDictionary, function, imageDictionary, buttonFormat, cloneFirstLinkage, true);
    }

	protected void AddButtonData(string list, string buttonName, Dictionary<string, string> buttonDictionary, string function, Dictionary<string, Dictionary<string, string>> imageDictionary, ListButtonData.ButtonFormat buttonFormat, GConnector cloneFirstLinkage, bool read)
	{
		ListButtonData data = new ListButtonData();
		data.buttonName = buttonName;
		data.connectionFunction = function;
		data.buttonFormat = buttonFormat.ToString();
		data.textDictionary = buttonDictionary;
		data.imageDictionary = imageDictionary;
        data.read = read;

		var buttonData = GetButtonData(list);

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
	
	//	protected void AddButtonData(string buttonName, Dictionary<string, string> buttonDictionary, string CallStaticFunction, ListButtonData.ButtonFormat buttonFormat, GConnector cloneFirstLinkage) {
//
//	}

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

	public void AddChallengeButtons(List<Challenge> challengeList, ListButtonData.ButtonFormat format) {
		AddChallengeButtons("default", challengeList, format);
	}

	public void AddChallengeButtons(string list, List<Challenge> challengeList, ListButtonData.ButtonFormat format) {
		for(int i=0; i<challengeList.Count; i++) {
			string buttonName = format.ToString() + i;
			Dictionary<string, string> challengeDictionary = new Dictionary<string, string>();
			challengeDictionary.Add("TitleText", challengeList[i].name);
			challengeDictionary.Add("DescriptionText", challengeList[i].description);
			challengeDictionary.Add("DeadlineText", "Challenge expires in " + "5 days");
			if(format != ListButtonData.ButtonFormat.FriendChallengeButton) {
				challengeDictionary.Add("PrizePotText", "Prize pot: " + challengeList[i].points_awarded);
				if(format == ListButtonData.ButtonFormat.CommunityChallengeButton) {
					challengeDictionary.Add("ExtraPrizeText", "Extra Prize: " + challengeList[i].prize);
				}
			}
			AddButtonData(list, buttonName, challengeDictionary, "", format, GetBaseButtonConnection());
			
		}
	}

	public GConnector GetConnection(string connectionName) {
		GConnector gc = Outputs.Find(r => r.Name == connectionName);
		if (gc == null)
		{
			UnityEngine.Debug.LogError("MobileSelectPanel: error finding connection - " + connectionName);
		}
		
//		DataVault.Set("facebook_message", "Connect to Facebook");
		
		return gc;
	}
}
