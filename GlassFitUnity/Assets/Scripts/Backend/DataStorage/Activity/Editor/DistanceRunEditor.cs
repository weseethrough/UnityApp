using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

/// <summary>
/// distance run inspector wrapper focused on managed time and distance display splitting single value into easily readable for human bits
/// </summary>
[CustomEditor(typeof(DistanceRun))]
public class DistanceRunEditor : Editor
{

    /// <summary>
    /// default unity function which draws inspector window components
    /// </summary>
    /// <returns></returns>
    public override void OnInspectorGUI()
    {
        DistanceRun script = (DistanceRun)target;
               
        script.targetTimeMS = InspectorHelpers.DrawTime("Target time:", script.targetTimeMS);        
        script.targetDistanceCM = InspectorHelpers.DrawDistance("Target distance:", script.targetDistanceCM);
    }
}