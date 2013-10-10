using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

//allows to mark game object to be serialized. In future it might contain special data, identification or serialization related custom information

public class UISerializable : MonoBehaviour 
{
    public bool keepAliveIfPossible = false;    
}
