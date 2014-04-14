using System;
using UnityEngine;
using System.Runtime.Serialization;
using System.Collections.Generic;

[Serializable]
public class SerializedNode : SerializedNodeBase
{

    public SerializedNode() : base()  { }

    public SerializedNode(GameObject source) : base(source) { }

    public SerializedNode(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) 
    {
        List<SerializedNode> list = (List<SerializedNode>)info.GetValue("SubBranches", typeof(List<SerializedNode>));

        this.subBranches  = list.ConvertAll(new Converter<SerializedNode, SerializedNodeBase>(LocalConverter));
    }
    
    public static SerializedNodeBase LocalConverter(SerializedNode data)
    {
        return (SerializedNodeBase)data;
    }
}

