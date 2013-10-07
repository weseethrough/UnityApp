using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(SimpleLocalizationManager))] 
public class SimpleLocalizationEditor : Editor 
{	

    public override void OnInspectorGUI () 
	{
		SimpleLocalizationManager manager = (SimpleLocalizationManager) target;
		
		manager.elementsExpand = EditorGUILayout.Foldout(manager.elementsExpand, "Localizations");
						
		if(manager.elementsExpand) 
		{			
			manager.elementsSize = EditorGUILayout.IntField("Size", manager.elementsSize);
			
			if (manager.elementsSize != manager.name.Count)
			{
				int targetSize = manager.elementsSize;
				int currentSize = manager.name.Count;
				while (targetSize > currentSize && targetSize < 1024)
				{
					manager.name.Add(string.Empty);
					manager.data.Add(string.Empty);
					currentSize ++;
				}
				
				if (targetSize > currentSize && targetSize >= 0)
				{
					manager.name.RemoveRange(targetSize, currentSize-targetSize);
					manager.data.RemoveRange(targetSize, currentSize-targetSize);				
				}
				
			}
			
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("String Key");
				EditorGUILayout.LabelField("Translation");
			EditorGUILayout.EndHorizontal();
			
			for (int i = 0; i < manager.name.Count; i++) 
			{
				EditorGUILayout.BeginHorizontal();
				manager.name[i] = EditorGUILayout.TextField(manager.name[i]);
				manager.data[i] = EditorGUILayout.TextField(manager.data[i]);
				EditorGUILayout.EndHorizontal();
			}
		}
		
		
        if (GUI.changed)
            EditorUtility.SetDirty (target);
    }
}
