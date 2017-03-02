using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class SceneView : InstanceView<Model.Scene>
{
    [SerializeField]
    private InstancePoolSetup imagesSetup;
    private InstancePool<Model.Image> images;

    private void Awake()
    {
        images = imagesSetup.Finalise<Model.Image>();
    }

    protected override void Configure()
    {
    }

    public override void Refresh()
    {
        images.SetActive(config.images);
        images.Refresh();
    }
}
