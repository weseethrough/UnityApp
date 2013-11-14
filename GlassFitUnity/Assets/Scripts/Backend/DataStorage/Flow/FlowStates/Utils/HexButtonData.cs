using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class HexButtonData : ISerializable 
{
    public string imageName = string.Empty;    

    public HexButtonData() { }
    public HexButtonData(SerializationInfo info, StreamingContext ctxt)         
    {
        this.imageName = Convert.ToString(info.GetValue("imageName", typeof(string)));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("imageName", this.imageName);
    }

}
