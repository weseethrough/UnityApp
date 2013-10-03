using UnityEngine;
using System.Collections;

public abstract class DataStoredType 
{
    public abstract byte[] Serialize();
    public abstract void DeSerialize(byte[] source);
}
