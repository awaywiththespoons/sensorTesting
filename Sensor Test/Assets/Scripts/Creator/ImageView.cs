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
    private Main main;
    [SerializeField]
    private Image image;
    [SerializeField]
    private Text text;
    [SerializeField]
    private Image selectedImage;

    public bool selected;
    public bool hidden;

    private void Update()
    {
        selectedImage.gameObject.SetActive(selected);
        selectedImage.color = Color.HSVToRGB(Time.timeSinceLevelLoad % 1, .25f, 1);

        text.color = image.color;

        text.enabled = config.text;
        image.color *= new Color(1, 1, 1, config.text ? 0 : 1);

        if (hidden)
        {
            float value = main.previewMode ? 0 : 0.25f;

            image.color *= value;
            text.color *= value;
        }

        if (config.text)
        {
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, text.preferredWidth);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, text.preferredHeight);
        }
    }

    protected override void Configure()
    {
        image.sprite = config.sprite;
        image.alphaHitTestMinimumThreshold = 0.25f;
        image.SetNativeSize();
    }

    public void SetFrame(float frame)
    {
        var rtrans = transform as RectTransform;

        int count = config.keyframes.Count;

        var next = config.keyframes[Mathf.CeilToInt(frame) % count];
        var prev = config.keyframes[Mathf.FloorToInt(frame) % count];
        float u = frame % 1;

        rtrans.position = Vector2.Lerp(prev.position, next.position, u);
        rtrans.eulerAngles = Mathf.LerpAngle(prev.direction, next.direction, u)
                           * Vector3.forward;
        rtrans.localScale = Mathf.Lerp(prev.scale, next.scale, u)
                          * Vector3.one;

        text.text = config.path;

        hidden = prev.hide;
    }
}
