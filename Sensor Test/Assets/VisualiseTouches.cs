using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class VisualiseTouches : MonoBehaviour 
{
    [SerializeField]
    private Image touchPrefab;
    [SerializeField]
    private Text debugText;

    public IndexedPool<Image> touchIndicators;

    private void Awake()
    {
        touchIndicators = new IndexedPool<Image>(touchPrefab, transform);
    }

    private void Update()
    {
        int count = Input.touchCount;

        debugText.text = string.Format("Touches: {0}", count);

        touchIndicators.SetActive(count);

        float sum = 0;

        for (int i = 0; i < count; ++i)
        {
            var touch = Input.GetTouch(i);

            touchIndicators[i].transform.position = touch.position;
            touchIndicators[i].color = Color.HSVToRGB(touch.fingerId / 10f, 1, 1);

            sum += touch.pressure;
        }

        debugText.text += string.Format(" - avg pressure {0}", sum / count);
    }
}
