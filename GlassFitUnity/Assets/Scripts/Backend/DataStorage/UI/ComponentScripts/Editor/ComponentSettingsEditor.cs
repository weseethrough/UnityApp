using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UIComponentSettings))]
public class ComponentSettingsEditor : Editor
{

    public override void OnInspectorGUI()
    {
        UIComponentSettings script = (UIComponentSettings)target;

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Component Label");
            script.textLabel = EditorGUILayout.TextArea(script.textLabel);
        EditorGUILayout.EndHorizontal();
    }

}