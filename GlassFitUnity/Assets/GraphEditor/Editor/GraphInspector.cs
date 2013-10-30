using UnityEditor;
using UnityEngine;
using System.Collections;

/// (C) Copyright 2013 by Paul C. Isaac 
[CustomEditor(typeof(GraphComponent))]
public class GraphInspector : Editor
{
	public override void OnInspectorGUI ()
	{
		//GraphComponent c = target as GraphComponent;
		
		EditorGUIUtility.LookLikeControls(144);

		DrawDefaultInspector();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(new GUIContent("View Graph","Open graph window")))
		{
			GraphWindow.Init();
			//desc.TextureCoords.Min = new Vector2(0,0);
			//desc.TextureCoords.Max = new Vector2(1,1);
			//EditorUtility.SetDirty(target);
		}
        if (GUILayout.Button(new GUIContent("Clear Graph", "Removes data stored in graph")))
        {
            GraphComponent gc = target as GraphComponent;
            gc.Data.ClearGraphData();
        }
        if (GUILayout.Button(new GUIContent("Reset Style", "Sets graph settings to default (skipps texture settings)")))
        {
            GraphComponent gc = target as GraphComponent;
            gc.Data.Style = new GStyle();
        }
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}
	
	/// <summary>
	/// Helper function to draw button in enabled or disabled state.
	/// </summary>
	static bool DrawButton (string title, string tooltip, bool enabled, float width)
	{
		if (enabled)
		{
			// Draw a regular button
			return GUILayout.Button(new GUIContent(title, tooltip), GUILayout.Width(width), GUILayout.Height(24));
		}
		else
		{
			// Button should be disabled -- draw it darkened and ignore its return value
			Color color = GUI.color;
			GUI.color = new Color(1f, 0.33f, 0.33f, 0.35f);
			GUILayout.Button(new GUIContent(title, tooltip), GUILayout.Width(width), GUILayout.Height(24));
			GUI.color = color;
			return false;
		}
	}
}
