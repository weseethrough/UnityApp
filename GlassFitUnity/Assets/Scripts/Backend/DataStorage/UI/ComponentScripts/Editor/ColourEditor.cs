using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// custom color inspector wrapper required for some function calls
/// </summary>
[CustomEditor(typeof(UIColour))]
public class ColourEditor : Editor
{
    /// <summary>
    /// default unity function which draws inspector window components. In this case there is required some preparation before UIColor can be used for display or calculations.
    /// </summary>
    /// <returns></returns>
    public override void OnInspectorGUI()
    {
        UIColour script = (UIColour)target;

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Component Color");
            Color oldColor = script.GetColor();
            Color newColor = EditorGUILayout.ColorField(oldColor);
            Vector4 vector = new Vector4(newColor.r, newColor.g, newColor.b, newColor.a);
            if (script.GetVector() != vector)
            {
                script.r = newColor.r;
                script.g = newColor.g;
                script.b = newColor.b;
                script.a = newColor.a;

                script.Apply();
            }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Database link name");
            script.databaseIDName = EditorGUILayout.TextArea(script.databaseIDName);
        EditorGUILayout.EndHorizontal();
    }

}