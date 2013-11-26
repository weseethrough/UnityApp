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
    public string imageName = string.Empty;
    public string buttonName = string.Empty;
    public bool expectedToHaveCustomExit = false;
    public int column = 0;
    public int row = 0;

    /// <summary>
    /// default constructor
    /// </summary>
    /// <returns></returns>
    public HexButtonData() { }

    /// <summary>
    /// deserialziation constructor
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
                case "expectedToHaveCustomExit":
                    this.expectedToHaveCustomExit = (bool)entry.Value;
                    break;
                case "column":
                    this.column = (int)entry.Value;
                    break;
                case "row":
                    this.row = (int)entry.Value;
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
        info.AddValue("expectedToHaveCustomExit", this.expectedToHaveCustomExit);
        info.AddValue("column", this.column);
        info.AddValue("row", this.row);
    }

}
