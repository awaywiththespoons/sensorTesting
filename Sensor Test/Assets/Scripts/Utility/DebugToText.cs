using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class DebugToText : MonoBehaviour 
{
    [SerializeField]
    private Text text;
    [SerializeField]
    private bool showTrace;

    private void Awake()
    {
        Application.logMessageReceived += (log, trace, type) =>
        {
            if (showTrace)
            {
                text.text += string.Format("{0}\n{1}\n", log, trace);
            }
            else
            {
                text.text += string.Format("{0}\n", log);
            }
        };
    }
}
