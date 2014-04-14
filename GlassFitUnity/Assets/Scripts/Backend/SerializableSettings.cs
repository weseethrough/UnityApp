using System;
using UnityEngine;
using System.Runtime.Serialization;
using System.Collections.Generic;

[Serializable]
public class SerializableSettings : SerializableSettingsBase
{

    public SerializableSettings() : base()  { }

    public SerializableSettings(GameObject source) : base(source) { }

    public SerializableSettings(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) 
    {
        List<SingleComponent> list = (List<SingleComponent>)info.GetValue("Components", typeof(List<SingleComponent>));

        this.components = list.ConvertAll(new Converter<SingleComponent, SingleComponentBase>(LocalConverter));
    }

    public static SingleComponentBase LocalConverter(SingleComponent data)
    {
        return (SingleComponentBase)data;    
    }
}

