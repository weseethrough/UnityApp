using System;
using System.Collections.Generic;
using UnityEngine;

class GameObjectUtils
{
    static public Transform FindChildInTree(Transform root, string childName)
    {
        Transform[] children = root.gameObject.GetComponentsInChildren<Transform>(true);
        
        foreach(Transform t in children)
        {
            if (t.name == childName)
            {
                return t;
            }
        }

        return null;
    }
}
