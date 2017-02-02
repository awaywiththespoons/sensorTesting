using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class SensorPlotter : MonoBehaviour 
{
    private struct Data
    {
        public Vector3 plot;
        public Color color;
    }

    [SerializeField]
    private Sensing sensor;
    [SerializeField]
    private SpriteRenderer plotTemplate;
    private IndexedPool<SpriteRenderer> plots;
    private List<Data> plotData = new List<Data>();

    private int count = 0;

    public void Increment()
    {
        count += 1;
    }

    public void Reset()
    {
        count = 0;
        plotData.Clear();
        plots.SetActive(0);
    }

    private void Awake()
    {
        plots = new IndexedPool<SpriteRenderer>(plotTemplate);
    }

    private void Update()
    {
        if (!sensor.valid)
        {
            return;
        }

        plotData.Add(new Data
        {
            color = Color.HSVToRGB(count / 10f, 1, 1),
            plot = Sensing.SortVectorComponents(sensor.feature),
        });

        float plotScale = 1;

        if (plotData.Count > 0)
        {
            float max = plotData.Select(data => Mathf.Max(data.plot.x, data.plot.y, data.plot.z)).Max();

            if (max > 0)
            {
                plotScale = 1 / max;
            }
        }

        plots.SetActive(plotData.Count);
        plots.MapActive((i, plot) =>
        {
            plot.transform.localPosition = plotData[i].plot * plotScale;
            plot.GetComponent<SpriteRenderer>().color = plotData[i].color;
        });
    }
}
