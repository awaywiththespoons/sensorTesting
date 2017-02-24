using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

public class CoroutineSingleton : MonoBehaviourSingleton<CoroutineSingleton>
{
    public static Coroutine Start(IEnumerator enumerator,
                                  Action OnCompleted=null)
    {
        return (instance as MonoBehaviour).StartCoroutine(StartCoroutineCO(enumerator, OnCompleted));
    }

    public new static void Stop(Coroutine coroutine)
    {
        if (coroutine == null) return;

        (instance as MonoBehaviour).StopCoroutine(coroutine);
    }

    private static IEnumerator StartCoroutineCO(IEnumerator enumerator,
                                                Action OnCompleted = null)
    {
        yield return (instance as MonoBehaviour).StartCoroutine(enumerator);

        if (OnCompleted != null) OnCompleted();
    }
}
