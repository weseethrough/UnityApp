using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UIBasiclabel))]
public class ComponentSettingsEditor : Editor
{

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