﻿using UnityEngine;
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
    private Main test;

    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Button selectButton;
    [SerializeField]
    private Button renameButton;

    private void Awake()
    {
        selectButton.onClick.AddListener(() => test.OpenStory(config));
        renameButton.onClick.AddListener(() => test.RenameStory(config));
    }

    protected override void Configure()
    {
        nameText.text = config;
    }
}
