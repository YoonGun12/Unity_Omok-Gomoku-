using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static GameObject FindChildObjectWithName(GameObject obj, string name,bool Recursive = false)
    {
        if (!Recursive)
        {
            foreach (Transform child in obj.transform)
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }
        }
        else
        {
            Transform[] allTransforms = obj.GetComponentsInChildren<Transform>();
            
            foreach (Transform child in allTransforms)
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }
        }

        Debug.Log($"Can't find {name} from {obj.name} ");
        return null;
    }
    public static T GetChildComponent<T>(GameObject obj, string name,bool Recursive = false) where T : Component
    {
        if (!Recursive)
        {
            foreach (Transform child in obj.transform)
            {
                T component = child.GetComponent<T>();
                if (child.name == name && component != null)
                {
                    return component;
                }
            }
        }
        else
        {
            T[] allTransforms = obj.GetComponentsInChildren<T>();
            
            foreach (T child in allTransforms)
            {
                if (child.gameObject.name == name)
                {
                    return child;
                }
            }
        }

        Debug.Log($"Can't find {name} from {obj.name} ");
        return null;
    }
}
