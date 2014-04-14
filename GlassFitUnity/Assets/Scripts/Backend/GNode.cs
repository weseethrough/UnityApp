using System;
using UnityEngine;
using System.Runtime.Serialization;

[Serializable]
public abstract class GNode : GNodeBase
{
    public GNode() : base() { }

    public GNode(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) { }
}

