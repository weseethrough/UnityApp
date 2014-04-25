using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System;
using RaceYourself.Models.Blob;

/// <summary>
/// single component contains all integer, boolean float, double and string parameters which were defined public on one component. It allows for storage for serialziation and deserialziation
/// </summary>
[Serializable]
public class SingleComponentBase
{
    public SDBase<int>      intData;
    public SDBase<double>   doubleData;
    public SDBase<string>   strData;
    public string               name;
    
    /// <summary>
    /// default constructor  and initializator
    /// </summary>    
    public SingleComponentBase()
    {
        this.intData = new SDBase<int>();
        this.doubleData = new SDBase<double>();
        this.strData = new SDBase<string>();
        name            = string.Empty;
    }    

    /// <summary>
    /// gives access to and initializes in necessary integer/boolean storage
    /// </summary>
    /// <returns>integer based SDBase storage</returns>
    public SDBase<int> GetInitializedIntDict()
    {
        if (intData == null) intData = new SDBase<int>();
        return intData;
    }

    /// <summary>
    /// gives access to and initializes in necessary float/double storage
    /// </summary>
    /// <returns>double based SDBase storage</returns>
    public SDBase<double> GetInitializedFloatDict()
    {
        if (doubleData == null) doubleData = new SDBase<double>();
        return doubleData;
    }

    /// <summary>
    /// gives access to and initializes in necessary string storage
    /// </summary>
    /// <returns>string based SDBase storage</returns>
    public SDBase<string> GetInitializedStrDict()
    {
        if (strData == null) strData = new SDBase<string>();
        return strData;
    }

    /// <summary>
    /// clones component data and all its parameters and settings
    /// </summary>
    /// <returns>returns copy of cloned class</returns>
    public SingleComponentBase Clone()
    {
        SingleComponentBase sc = new SingleComponentBase();
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
