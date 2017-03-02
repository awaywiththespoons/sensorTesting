using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class Test : MonoBehaviour 
{
    [SerializeField]
    private SceneView scene;

    [SerializeField]
    private Model.Scene data;

    private void Start()
    {
        scene.SetConfig(data);
    }

    private void Update()
    {
        scene.Refresh();
    }
}
