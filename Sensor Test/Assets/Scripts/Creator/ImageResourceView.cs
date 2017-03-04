using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class ImageResourceView : InstanceView<Test.ImageResource> 
{
    [SerializeField]
    private Test test;
    [SerializeField]
    private Button selectButton;
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Image image;

    private void Start()
    {
        selectButton.onClick.AddListener(() => test.AddImageResource(config));
    }

    protected override void Configure()
    {
        base.Configure();

        nameText.text = config.name;
        image.sprite = config.sprite;
    }
}
