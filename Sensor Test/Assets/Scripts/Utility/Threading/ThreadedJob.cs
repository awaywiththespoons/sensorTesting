using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

public class ThreadedJob
{
    public static void Run<T>(Action<T> OnSetup, 
                              Action<T> OnCompleted)
        where T : ThreadedJob, new()
    {
        var job = new T();

        OnSetup(job);

        job.Start();

        CoroutineSingleton.Start(job.WaitFor(), () => OnCompleted(job));
    }

    private object @lock = new object();
    private Thread thread = null;

    private bool _finished = false;
    public bool finished
    {
        get
        {
            bool tmp;
            lock (@lock)
            {
                tmp = _finished;
            }
            return tmp;
        }
        set
        {
            lock (@lock)
            {
                _finished = value;
            }
        }
    }

    public virtual void Start()
    {
        thread = new Thread(Run);
        thread.Start();
    }

    public virtual void Abort()
    {
        thread.Abort();
    }

    protected virtual void ThreadFunction() { }

    protected virtual void OnFinished() { }

    public virtual bool Update()
    {
        if (finished)
        {
            OnFinished();
            return true;
        }
        return false;
    }

    IEnumerator WaitFor()
    {
        while (!Update())
        {
            yield return null;
        }
    }

    private void Run()
    {
        ThreadFunction();
        finished = true;
    }
}