using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

public class BackendWindow : EditorWindow
{
	string myString = "Hello World";
	bool groupEnabled;
	bool myBool = true;
	float myFloat = 1.23f;
	
	[MenuItem("Window/Glass Fit BackendWindow")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(BackendWindow));
	}

	void OnGUI()
	{
		GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
		if (GUILayout.Button("Initialize") && DataStorage.instance != null)
		{
			Debug.Log("Data storage initialziation!");
			DataStorage.instance.Initialize(null);
		}
	}
}
