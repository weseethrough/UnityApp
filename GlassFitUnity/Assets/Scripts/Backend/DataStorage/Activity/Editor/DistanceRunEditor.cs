using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

[CustomEditor(typeof(DistanceRun))]
public class DistanceRunEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DistanceRun script = (DistanceRun)target;
               
        script.targetTimeMS = InspectorHelpers.DrawTime("Target time:", script.targetTimeMS);        
        script.targetDistanceCM = InspectorHelpers.DrawDistance("Target distance:", script.targetDistanceCM);
    }
}