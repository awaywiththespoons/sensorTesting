using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Useful for storing hard coded config that can be exposed through the editor
/// so it doesn't have to be put onto a prefab or scene object - this way it
/// doesn't rely on the scene.
/// 
/// Perhaps not an ideal solution.
/// 
/// Usage:
/// public class Thing : ScriptableObjectSingleton<Thing> { }
/// </summary>
public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance;
    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                T[] instances = Resources.LoadAll<T>("Singletons");

                if (instances.Length == 0)
                {
                    Debug.LogError("No instance found for singleton " + typeof(T).Name);
                }
                else if (instances.Length > 1)
                {
                    Debug.LogError("Multiple instances found for singleton" + typeof(T).Name);
                }
                else
                {
                    _instance = instances[0];
                    (_instance as ScriptableObjectSingleton<T>).Initialise();
                }
            }

            return _instance;
        }
    }

    public virtual void Initialise()
    {
    }
}
