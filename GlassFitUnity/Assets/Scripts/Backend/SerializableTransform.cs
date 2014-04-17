using System;
using UnityEngine;
using System.Runtime.Serialization;

[Serializable]
public class SerializableTransform : SerializableTransformBase
{
    public SerializableTransform() : base() { }

    public SerializableTransform(Transform source) : base(source) { }

    public SerializableTransform(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) { }
}

