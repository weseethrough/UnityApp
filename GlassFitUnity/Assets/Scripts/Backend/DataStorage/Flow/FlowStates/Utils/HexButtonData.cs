using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// serializable data for dynamic hex list
/// </summary>
[Serializable]
public class HexButtonData : ISerializable 
{
    public const string  NO_IMAGE   = "!none";

    public string   imageName       = NO_IMAGE;
    public string   buttonName      = string.Empty;
    public string   activityName    = "default activity name";
    public string   activityContent = "default activity content";
    public string   onButtonCustomString = string.Empty;
    public int      column          = 0;
    public int      row             = 0;
    public int      activityPrice   = 12345;
    public int      count           = -1;
    public bool     locked          = false;
    public bool     displayInfoData = true;
    public bool     displayPlusMarker = false;
    public bool     allowEarlyHover = false;
        
    /// <summary>
    /// default constructor
    /// </summary>
    /// <returns></returns>
    public HexButtonData() { }

    /// <summary>
    /// deserialization constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public HexButtonData(SerializationInfo info, StreamingContext ctxt)         
    {
        foreach (SerializationEntry entry in info)
        {
            switch (entry.Name)
            {
                case "imageName":
                    this.imageName = entry.Value as String;
                    break;
                case "buttonName":
                    this.buttonName = entry.Value as String;
                    break;
                case "activityName":
                    this.activityName = entry.Value as String;
                    break;
                case "activityContent":
                    this.activityContent = entry.Value as String;
                    break;                
                case "column":
                    this.column = (int)entry.Value;
                    break;
                case "row":
                    this.row = (int)entry.Value;
                    break;
                case "activityPrice":
                    this.activityPrice = (int)entry.Value;
                    break;
                case "locked":
                    this.locked = (bool)entry.Value;
                    break;
                case "displayInfoData":
                    this.displayInfoData = (bool)entry.Value;
                    break;
                case "displayPlusMarker":
                    this.displayPlusMarker = (bool)entry.Value;
                    break;
                case "allowEarlyHover":
                    this.allowEarlyHover = (bool)entry.Value;
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
        info.AddValue("imageName", this.imageName);
        info.AddValue("buttonName", this.buttonName);        
        info.AddValue("column", this.column);
        info.AddValue("row", this.row);
        info.AddValue("locked", this.locked);
        info.AddValue("activityName", this.activityName);
        info.AddValue("activityContent", this.activityContent);
        info.AddValue("activityPrice", this.activityPrice);
        info.AddValue("displayInfoData", this.displayInfoData);
        info.AddValue("displayPlusMarker", this.displayPlusMarker);
        info.AddValue("allowEarlyHover", this.allowEarlyHover);
    }

}
