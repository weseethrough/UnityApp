using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class HexButtonData : ISerializable 
{
    public string imageName = string.Empty;
    public string buttonName = string.Empty;
    public bool expectedToHaveCustomExit = false;
    public int column = 0;
    public int row = 0;

    public HexButtonData() { }
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

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("imageName", this.imageName);
        info.AddValue("buttonName", this.buttonName);
        info.AddValue("expectedToHaveCustomExit", this.expectedToHaveCustomExit);
        info.AddValue("column", this.column);
        info.AddValue("row", this.row);
    }

}
