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
    private Main test;

    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Button selectButton;
    [SerializeField]
    private Button renameButton;
    [SerializeField]
    private Button deleteButton;
    [SerializeField]
    private Button duplicateButton;
    [SerializeField]
    private Button toggleLockButton;
    [SerializeField]
    private GameObject unlockedButtons;

    private void Awake()
    {
        selectButton.onClick.AddListener(() => test.OpenStory(config));
        renameButton.onClick.AddListener(() => test.RenameStory(config));
        deleteButton.onClick.AddListener(() => test.RemoveStory(config));
        duplicateButton.onClick.AddListener(() => test.DuplicateStory(config));
        toggleLockButton.onClick.AddListener(() => test.ToggleLockStory(config));
    }

    protected override void Configure()
    {
        bool locked = config.EndsWith("_L");

        unlockedButtons.SetActive(!locked);

        nameText.text = locked 
                      ? config.Substring(0, config.Length - 2) 
                      : config;    
    }
}
