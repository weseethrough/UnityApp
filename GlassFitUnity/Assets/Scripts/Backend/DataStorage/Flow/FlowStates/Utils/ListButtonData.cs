using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// serializable data for dynamic list
/// </summary>
[Serializable]
public class ListButtonData : ISerializable 
{      
    public enum ButtonFormat
    {
        ButtonNormalPrototype,
        ButtonPrototype,
        SliderPrototype,
		ChallengeButton,
		InviteButton,
		ImportButton,
		ActiveChallengeButton,
		CommunityChallengeButton,
		FriendChallengeButton,
		NewChallengeButton,
		InvitePromptButton,
		InvitedButton
    }

    public string   buttonName      = string.Empty;    
    public string   textNormal      = string.Empty;
    public string   connectionFunction = string.Empty;
    public string   buttonFormat = ButtonFormat.ButtonNormalPrototype.ToString();
	public string 	imageName		= string.Empty;

	public Dictionary<string, string> attributeDictionary = new Dictionary<string, string>();

    /// <summary>
    /// default constructor
    /// </summary>
    /// <returns></returns>
    public ListButtonData() { }

    /// <summary>
    /// deserialization constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public ListButtonData(SerializationInfo info, StreamingContext ctxt)         
    {
        foreach (SerializationEntry entry in info)
        {
            switch (entry.Name)
            {               
                case "buttonName":
                    this.buttonName = entry.Value as String;
                    break;               
                case "textNormal":
                    this.textNormal = entry.Value as String;
                    break;
                case "connectionFunction":
                    this.connectionFunction = entry.Value as String;
                    break;
                case "buttonFormat":
                    this.buttonFormat = entry.Value as String;
                    break;
                
            }
        }
    }

    /// <summary>
    /// serialization function called by serializer
    /// </summary>
    /// <param name="info">serialziation info where all data would be pushed to</param>
    /// <param name="ctxt">serialzation context</param>
    /// <returns></returns>
    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {        
        info.AddValue("buttonName", this.buttonName);                
        info.AddValue("textNormal", this.textNormal);
        info.AddValue("connectionFunction", this.connectionFunction);
        info.AddValue("buttonFormat", this.buttonFormat);
		info.AddValue("imageName", this.imageName);
    }

    /// <summary>
    /// Makes a copy of this button data
    /// </summary>
    /// <returns></returns>
    public ListButtonData Copy()
    {
        ListButtonData data = new ListButtonData();
        data.buttonName = buttonName;
        data.textNormal = textNormal;
        data.connectionFunction = connectionFunction;
        data.buttonFormat = buttonFormat;
		data.imageName = imageName;

        return data;
    }
}
