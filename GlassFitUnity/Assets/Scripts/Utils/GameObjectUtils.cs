using UnityEngine;
using System.Collections;
using System;

public class GameObjectUtils 
{
    static public T	GetComponentByName<T>(GameObject root, string ownerUniqueName) where T: Component
    {       
        T[] components = root.GetComponentsInChildren<T>(true);

        foreach(T component in components)
        {
            if (component.gameObject.name == ownerUniqueName)
            {
                return component;
            }
        }

       return null;
    }

    static public void SetTextOnLabelInChildren(GameObject root, string labelName, string textToSet)
    {
        UILabel label = GetComponentByName<UILabel>(root, labelName);
        if (label != null)
        {
            label.text = textToSet;
            label.MarkAsChanged();
        }
    }

    static public GameObject SearchTreeByName(GameObject root, string uniqueName)
    {
        Transform[] components = root.GetComponentsInChildren<Transform>() as Transform[];

        foreach (Transform component in components)
        {
            if (component.gameObject.name == uniqueName)
            {
                return component.gameObject;
            }
        }

        return null;
    }
}
