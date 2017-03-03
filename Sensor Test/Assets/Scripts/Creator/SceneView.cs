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
    private CanvasGroup canvasGroup;

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

    public void SetFrame(float frame)
    {
        frame %= config.frameCount;

        images.MapActive((i, image) => image.SetFrame(frame));
    }

    private float fadeTime = .25f;
    private float fadeVelocity;

    [ContextMenu("Fade In")]
    public void FadeIn()
    {
        gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(FadeInCO());
    }

    [ContextMenu("Fade Out")]
    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutCO());
    }

    private IEnumerator FadeInCO()
    {
        fadeVelocity = 0;

        while (canvasGroup.alpha < 0.99f)
        {
            canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha,
                                                 1f,
                                                 ref fadeVelocity,
                                                 fadeTime);

            yield return null;
        }

        canvasGroup.alpha = 1;
    }

    private IEnumerator FadeOutCO()
    {
        fadeVelocity = 0;

        while (canvasGroup.alpha > 0.01f)
        {
            canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha,
                                                 0f,
                                                 ref fadeVelocity,
                                                 fadeTime);

            yield return null;
        }

        canvasGroup.alpha = 0;

        gameObject.SetActive(false);
    }
}
