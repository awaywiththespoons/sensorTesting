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
    public IndexedPool<Image> touchIndicators2;

    private void Awake()
    {
        touchIndicators = new IndexedPool<Image>(touchPrefab, transform);
        touchIndicators2 = new IndexedPool<Image>(touchPrefab, transform);
    }

    private void Update()
    {
        int count = Input.touchCount;

        debugText.text = string.Format("Touches: {0}", count);

        touchIndicators.SetActive(count);
        touchIndicators2.SetActive(count);

        for (int i = 0; i < count; ++i)
        {
            var touch = Input.GetTouch(i);

            touchIndicators[i].transform.position = touch.position;
            touchIndicators[i].color = Color.HSVToRGB(touch.fingerId / 10f, .75f, 1);
            touchIndicators[i].GetComponentInChildren<Text>().text = touch.fingerId.ToString();

            touchIndicators2[i].transform.position = touch.position * 0.2f;
            touchIndicators2[i].GetComponentInChildren<Text>().text = touch.fingerId.ToString();
            touchIndicators2[i].transform.localScale = Vector3.one * 0.2f;
            touchIndicators2[i].color = Color.HSVToRGB(touch.fingerId / 10f, 1, 1);
        }
    }
}
