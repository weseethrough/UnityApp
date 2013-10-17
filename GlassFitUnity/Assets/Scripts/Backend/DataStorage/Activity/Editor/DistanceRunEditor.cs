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

        
        EditorGUILayout.LabelField("Target time");
        
        EditorGUILayout.BeginHorizontal();
            int partTaken = 0;
            int hours = (int)(script.targetTimeSec / 3600.0f);
            partTaken = hours * 3600;
            int minutes = (int)((script.targetTimeSec - partTaken) / 60.0f);
            partTaken += minutes * 60;
            int seconds = (int)((script.targetTimeSec - partTaken));
            partTaken += seconds;

            //centysecond is 1/100 of the second. we do not count 1/1000 of the seconds!
            int centySeconds = (int)((script.targetTimeSec - (float)partTaken) * 100.0f );
            EditorGUILayout.LabelField("Time:");
            hours = EditorGUILayout.IntField(hours);
            EditorGUILayout.LabelField(":");
            minutes = EditorGUILayout.IntField(minutes);
            EditorGUILayout.LabelField(":");
            seconds = EditorGUILayout.IntField(seconds);
            EditorGUILayout.LabelField(".");
            string cs = EditorGUILayout.TextField(centySeconds.ToString("C2"));

            if (cs.Length > 1)
            {
                centySeconds = Convert.ToInt32(cs[0] + cs[1]);
            }
            else if (cs.Length == 1)
            {
                centySeconds = Convert.ToInt32(cs[0]);
            }
            else
            {
                centySeconds = 0;
            }
            

            script.targetTimeSec = (float)(hours * 3600 + minutes * 60 + seconds) + (float)centySeconds * 0.01f;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Target distance");
        
    }

}