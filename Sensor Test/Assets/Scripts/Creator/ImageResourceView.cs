using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class ImageResourceView : InstanceView<TestImageBrowser.ImageResource> 
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Image image;

    protected override void Configure()
    {
        base.Configure();

        nameText.text = config.name;
        image.sprite = config.sprite;
    }
}
