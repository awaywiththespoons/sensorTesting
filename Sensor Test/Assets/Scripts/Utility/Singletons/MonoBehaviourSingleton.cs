using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// For manager type components that are guaranteed to always be available in
/// the scene.
/// 
/// Generally not a flexible solution to problems.
/// 
/// Usage:
/// public class Thing : MonoBehaviourSingleton<Thing> { }
/// </summary>
public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] protected bool initialiseOnAwake = true;

    public static T instance { get; private set; }

    public virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;

            if (initialiseOnAwake) 
				Initialise();
        }
        else
        {
            Debug.LogError("Duplicate MonoBehaviourSingleton instances:");
            Debug.LogError(instance.name, instance);
            Debug.LogError(name, this);
        }
    }

    public static void Find()
    {
        instance = FindObjectOfType<T>();
    }

    protected virtual void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public virtual void Initialise()
    {
		
    }
}
