using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// component marking gameobject as prefab root and requesting serialization process on this level (including all other components laying along with it
/// </summary>
public class UISerializable : MonoBehaviour 
{
    public bool exposeInFlow = false;
}
