using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// custom label inspector wrapper required for some function calls whenever value changes and proper display of text in the interests
/// </summary>
[CustomEditor(typeof(UIBasiclabel))]
public class BasicLabelEditor : Editor
{
    /// <summary>
    /// default unity function which draws inspector window components
    /// </summary>
    /// <returns></returns>
    public override void OnInspectorGUI()
    {
        UIBasiclabel script = (UIBasiclabel)target;

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Component Label");
            string l = script.label;
            script.label = EditorGUILayout.TextArea(script.label);
            if (l != script.label) script.Apply();
        EditorGUILayout.EndHorizontal();
    }


}