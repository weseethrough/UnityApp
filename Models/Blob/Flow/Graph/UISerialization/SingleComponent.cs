using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System;
using RaceYourself.Models.Blob;

#if UNITY_EDITOR
using UnityEditor; 
using System.Reflection;
#endif


/// <summary>
/// single component contains all integer, boolean float, double and string parameters which were defined public on one component. It allows for storage for serialziation and deserialziation
/// </summary>
[Serializable]
public class SingleComponent
{
    public Dictionary<int>      intData;
    public Dictionary<double>   doubleData;
    public Dictionary<string>   strData;
    public string               name;
    
    /// <summary>
    /// default constructor  and initializator
    /// </summary>    
    public SingleComponent()
    {
        this.intData = new Dictionary<int>();
        this.doubleData = new Dictionary<double>();
        this.strData = new Dictionary<string>();
        name            = string.Empty;
    }    

    /// <summary>
    /// gives access to and initializes in necessary integer/boolean storage
    /// </summary>
    /// <returns>integer based dictionary storage</returns>
    public Dictionary<int> GetInitializedIntDict()
    {
        if (intData == null) intData = new Dictionary<int>();
        return intData;
    }

    /// <summary>
    /// gives access to and initializes in necessary float/double storage
    /// </summary>
    /// <returns>double based dictionary storage</returns>
    public Dictionary<double> GetInitializedFloatDict()
    {
        if (doubleData == null) doubleData = new Dictionary<double>();
        return doubleData;
    }

    /// <summary>
    /// gives access to and initializes in necessary string storage
    /// </summary>
    /// <returns>string based dictionary storage</returns>
    public Dictionary<string> GetInitializedStrDict()
    {
        if (strData == null) strData = new Dictionary<string>();
        return strData;
    }

    /// <summary>
    /// clones component data and all its parameters and settings
    /// </summary>
    /// <returns>returns copy of cloned class</returns>
    public SingleComponent Clone()
    {
        SingleComponent sc = new SingleComponent();
        if (intData != null)
        {
            sc.intData = intData.Clone();
        }
        if (doubleData != null)
        {
            sc.doubleData = doubleData.Clone();
        }
        if (strData != null)
        {
            sc.strData = strData.Clone();
        }

        sc.name = name;
        return sc;
    }
}
