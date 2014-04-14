using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System;

/// <summary>
/// this class is used instead of dictionary which is not serializable. It allows to have undefined set of serializable objects identified by name
/// Designed to store basic types (int, double, float, bool, string...)
/// </summary>
[Serializable]
public class StorageDictionaryBase<T> : SDBase<T> 
{
    public StorageDictionaryBase() : base()  { }

    public StorageDictionaryBase(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) { }
}
