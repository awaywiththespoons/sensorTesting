﻿using UnityEngine;
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
    private Canvas sceneCanvas;

    [SerializeField]
    private SceneView scene;

    [SerializeField]
    private Model.Scene data;

    [SerializeField]
    private bool play;
    [SerializeField]
    private bool angle;

    [SerializeField]
    [Range(0, 1)]
    private float playbackSpeed = 1;

    private void Start()
    {
        data.SetFrameCount(30);
        scene.SetConfig(data);

        timelineSlider.maxValue = data.frameCount;
    }

    private int fps = 10;

    private void Update()
    {
        if (play)
        {
            timelineSlider.value = (timelineSlider.value + Time.deltaTime * fps * playbackSpeed) % data.frameCount;

            int frame = Mathf.CeilToInt(timelineSlider.value);

            frame %= data.images[0].frameCount;

            if (Input.GetMouseButton(0))
            {
                if (angle)
                {
                    var center = new Vector2(Camera.main.pixelWidth / 2,
                                             Camera.main.pixelHeight / 2);

                    Vector2 direction = (Vector2) Input.mousePosition - center;

                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    data.images[0].directions[frame] = angle;
                }
                else
                {
                    data.images[0].positions[frame] = Input.mousePosition;
                }
            }
        }

        scene.SetFrame(timelineSlider.value);
    }
}
