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
        public int id;
        public Vector3 plot;
        public Color color;
    }

    [SerializeField]
    private Sensing sensor;
    [SerializeField]
    private SpriteRenderer plotTemplate;
    private IndexedPool<SpriteRenderer> plots;
    private List<Data> plotData = new List<Data>();

    [SerializeField]
    private SpriteRenderer sensorPlot;

    private int count = 0;
    private bool classifyMode;

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

    public void SetClassify(bool classify)
    {
        classifyMode = classify;
    }

    private void Awake()
    {
        plots = new IndexedPool<SpriteRenderer>(plotTemplate);
    }

    private void Update()
    {
        sensorPlot.gameObject.SetActive(classifyMode);

        float plotScale = 1;

        if (plotData.Count > 0)
        {
            float max = plotData.Select(data => Mathf.Max(data.plot.x, data.plot.y, data.plot.z)).Max();

            if (max > 0)
            {
                plotScale = 1 / max;
            }
        }

        if (!sensor.valid)
        {
            return;
        }
        else if (classifyMode)
        {
            Vector3 feature = Sensing.SortVectorComponents(sensor.feature);

            var ordered = plotData.OrderBy(plot => (feature - plot.plot).magnitude).ToList();
            var closest = ordered.Take(16).ToList();
            var best = Enumerable.Range(0, 10)
                                 .OrderByDescending(id => closest.Count(plot => plot.id == id))
                                 .First();

            Debug.Log(best);

            sensorPlot.transform.localPosition = feature * plotScale;
            sensorPlot.color = Color.HSVToRGB(best / 10f, 1, 1);

            return;
        }

        plotData.Add(new Data
        {
            id = count,
            color = Color.HSVToRGB(count / 10f, 1, 1),
            plot //= Sensing.SortVectorComponents(new Vector3(Random.value, Random.value, Random.value)),
             = Sensing.SortVectorComponents(sensor.feature),
        });

        plots.SetActive(plotData.Count);
        plots.MapActive((i, plot) =>
        {
            plot.transform.localPosition = plotData[i].plot * plotScale;
            plot.GetComponent<SpriteRenderer>().color = plotData[i].color;
        });
    }
}
