using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class StoryBrowserItem : InstanceView<string> 
{
    [SerializeField]
    private Test test;

    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Button selectButton;

    private void Awake()
    {
        selectButton.onClick.AddListener(() => test.OpenStory(config));
    }

    protected override void Configure()
    {
        nameText.text = config;
    }
}
