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
    private Slider timelineSlider;

    [SerializeField]
    private SceneView scene;

    [SerializeField]
    private Model.Scene data;

    [SerializeField]
    private bool play;

    private void Start()
    {
        data.SetFrameCount(3);
        scene.SetConfig(data);

        timelineSlider.maxValue = data.frameCount;
    }

    private void Update()
    {
        scene.SetFrame(timelineSlider.value);

        if (play)
        {
            timelineSlider.value = (timelineSlider.value + Time.deltaTime) % data.frameCount;
        }
    }
}
