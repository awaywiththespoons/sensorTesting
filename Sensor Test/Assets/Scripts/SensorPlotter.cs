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
    private List<int> classifications = new List<int>();
    private Dictionary<int, Vector3> meanSorted = new Dictionary<int, Vector3>();

    public void Increment()
    {
        count += 1;
    }

    public void Reset()
    {
        count = 0;
        plotData.Clear();
        plots.SetActive(0);
        meanSorted.Clear();
    }

    public void SetClassify(bool classify)
    {
        classifyMode = classify;
        classifications.Clear();
    }

    private void Awake()
    {
        plots = new IndexedPool<SpriteRenderer>(plotTemplate);
    }

    private bool explore = true;

    private void Update()
    {
        if (explore)
        {
            var triangle = Sensing.ExtractSidesFeature(new Sensing.TouchFrame
            {
                touches = new List<Vector2>
                {
                    new Vector2(Random.value, Random.value),
                    new Vector2(Random.value, Random.value),
                    new Vector2(Random.value, Random.value),
                },
            });

            plotData.Add(new Data
            {
                id = 0,
                color = Color.white,
                plot = triangle,
            });
        }

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

        if (!explore && !sensor.valid)
        {
            return;
        }
        else if (classifyMode)
        {
            Vector3 feature = Sensing.MinimiseVectorRank(sensor.feature);

            var ordered = plotData.OrderBy(plot => (feature - plot.plot).magnitude).ToList();
            var closest = ordered.Take(16).ToList();
            var best = Enumerable.Range(0, 10)
                                 .OrderByDescending(id => closest.Count(plot => plot.id == id))
                                 .First();

            classifications.Add(best);

            best = Enumerable.Range(0, 10)
                             .OrderByDescending(id => classifications.Count(c => c == id))
                             .First();

            Debug.LogFormat("{0} - {1}", feature, best);

            sensorPlot.transform.localPosition = feature * plotScale;
            sensorPlot.color = Color.HSVToRGB(best / 10f, 1, 1);

            return;
        }

        if (!explore)
        {
            plotData.Add(new Data
            {
                id = count,
                color = Color.HSVToRGB(count / 10f, 1, 1),
                plot = sensor.feature,
            });

            plotData.Add(new Data
            {
                id = count,
                color = Color.HSVToRGB(count / 10f, 1, 1),
                plot = Sensing.CycleVectorComponents(sensor.feature),
            });

            plotData.Add(new Data
            {
                id = count,
                color = Color.HSVToRGB(count / 10f, 1, 1),
                plot = Sensing.CycleVectorComponents(Sensing.CycleVectorComponents(sensor.feature)),
            });
        }

        /*
        Vector3 mean;

        if (!meanSorted.TryGetValue(count, out mean))
        {
            meanSorted[count] = sensor.feature;
            mean = sensor.feature;
        }

        meanSorted[count] = Vector3.Lerp(mean, Sensing.CycleVectorToMatchOther(sensor.feature, mean), 0.5f);

        for (int i = 0; i <= count; ++i)
        {
            Debug.LogFormat("{0} - {1}", i, meanSorted[i]);
        }

        sensorPlot.gameObject.SetActive(true);
        sensorPlot.transform.localPosition = Sensing.MinimiseVectorRank(meanSorted[count]) * plotScale;
        */

        plots.SetActive(plotData.Count);
        plots.MapActive((i, plot) =>
        {
            plot.transform.localPosition = plotData[i].plot * plotScale;
            plot.GetComponent<SpriteRenderer>().color = plotData[i].color;
        });
    }
}
