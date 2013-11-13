using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class ButtonFunctionCollection  
{
   
    /// <summary>
    /// Every function in collection have to accept FlowButton variable and return boolean. 
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <returns> Is button in state to continue? If False is returned button will not navigate forward on its own connection!</returns>
    static public bool MyFunction1(FlowButton fb)
    {
        Debug.Log("Testing linked function");

        return true;
    }

    static public bool MyFunction2(FlowButton fb)
    {
        Debug.Log("Testing linked function number2");

        return false;
    }

    public void Pure()
    {
        Debug.Log("Testing linked function number2");

        return;
    }
}
