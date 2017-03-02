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
    private ImageView imageTemplate;
    private IndexedPool<ImageView> images;

    private void Awake()
    {
        images = new IndexedPool<ImageView>(imageTemplate);
    }

    protected override void Configure()
    {
        images.SetActive(config.images.Count);
        images.MapActive((i, image) => image.SetConfig(config.images[i]));
    }

    public override void Refresh()
    {
        float frame = Time.timeSinceLevelLoad;

        images.MapActive((i, image) => image.SetFrame(frame));
    }
}
