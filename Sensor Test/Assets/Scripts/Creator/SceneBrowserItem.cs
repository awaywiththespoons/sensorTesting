using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class SceneBrowserItem : InstanceView<Model.Scene> 
{
    [SerializeField]
    private Test test;

    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Button selectButton;

    private void Awake()
    {
        selectButton.onClick.AddListener(() => test.OpenEditScene(config));
    }

    protected override void Configure()
    {
        nameText.text = string.Format("Edit {0}", config.name);
    }
}
