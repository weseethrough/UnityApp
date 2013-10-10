using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ComponentSettings))]
public class ComponentSettingsEditor : Editor
{

    public override void OnInspectorGUI()
    {
        ComponentSettings script = (ComponentSettings)target;

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Component Label");
            script.buttonLabel = EditorGUILayout.TextArea(script.buttonLabel);
        EditorGUILayout.EndHorizontal();
    }

}