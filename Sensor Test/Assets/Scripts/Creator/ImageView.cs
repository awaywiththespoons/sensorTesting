﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class ImageView : InstanceView<Model.Image> 
{
    [SerializeField]
    private Image image;

    public bool selected;

    private void Update()
    {
        if (selected)
        {
            image.color = Color.HSVToRGB(Time.timeSinceLevelLoad % 1, .25f, 1);
        }
        else
        {
            image.color = Color.white;
        }
    }

    protected override void Configure()
    {
        image.sprite = config.sprite;
        image.SetNativeSize();
    }

    public void SetFrame(float frame)
    {
        var rtrans = transform as RectTransform;

        int next = Mathf.CeilToInt(frame) % config.frameCount;
        int prev = Mathf.FloorToInt(frame) % config.frameCount;
        float u = frame % 1;

        rtrans.position = Vector2.Lerp(config.positions[prev],
                                       config.positions[next],
                                       u);
        rtrans.eulerAngles = Mathf.LerpAngle(config.directions[prev],
                                             config.directions[next],
                                             u)
                           * Vector3.forward;
        rtrans.localScale = Mathf.Lerp(config.scales[prev],
                                       config.scales[next],
                                       u)
                          * Vector3.one;
    }
}
