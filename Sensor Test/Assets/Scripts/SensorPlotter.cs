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
    private class Token
    {
        public Vector3 mean;
        public List<Vector3> training = new List<Vector3>();
    }

    private struct Data
    {
        public int id;
        public Vector3 plot;
        public Color color;
    }

    [SerializeField]
    private Sensing sensor;
    [SerializeField]
    private Sensor sensor2;
    [SerializeField]
    private SpriteRenderer plotTemplate;
    private IndexedPool<SpriteRenderer> plots;
    private List<Data> plotData = new List<Data>();

    [SerializeField]
    private SpriteRenderer sensorPlot;

    [SerializeField]
    private Text debugText;
    [SerializeField]
    private LineRenderer triangleRenderer;
    [SerializeField]
    private RectTransform shapeCanvas;

    private List<Token> tokens = new List<Token>();

    private int count = 0;
    private bool classifyMode;
    private List<int> classifications = new List<int>();
    private Dictionary<int, Vector3> meanSorted = new Dictionary<int, Vector3>();

    private Queue<Vector3> currentData = new Queue<Vector3>();
    //private int limit = 64;

    public void Increment()
    {
        Vector3 mean = Vector3.zero;

        foreach (var data in currentData)
        {
            mean += data;
        }

        mean /= currentData.Count;

        tokens.Add(new Token
        {
            mean = mean,
            training = currentData.ToList(),
        });

        count += 1;
        currentData.Clear();

        sensor2.SetTraining(new Sensor.Token { id = count });
    }

    public void Reset()
    {
        count = 0;
        plotData.Clear();
        plots.SetActive(0);
        meanSorted.Clear();

        currentData.Clear();
        tokens.Clear();

        sensor2.Reset();
        sensor2.SetTraining(new Sensor.Token { id = count });
    }

    public void SetClassify(bool classify)
    {
        classifyMode = classify;
        classifications.Clear();
        currentData.Clear();

        sensor2.SetClassify();
    }

    private void Awake()
    {
        plots = new IndexedPool<SpriteRenderer>(plotTemplate);

        Reset();
    }

    private bool explore = false;

    private static List<Vector2> SidesToTriangle(Vector2 position, Vector3 sides)
    {
        sides = -Triangle.CycleMinimiseX(-sides);

        Vector2 a = Vector2.zero;
        Vector2 b = a + Vector2.right * sides.x;

        float angle = -Mathf.Acos((sides.x * sides.x + sides.y * sides.y - sides.z * sides.z) / (2 * sides.x * sides.y));

        Vector2 c = a + new Vector2(Mathf.Cos(angle) * sides.y,
                                    Mathf.Sin(angle) * sides.y);

        Vector2 centroid = (a + b + c) / 3f;

        return new List<Vector2> { a - centroid + position,
                                   b - centroid + position,
                                   c - centroid + position };
    }

    private static List<Vector2> CompleteTriangle(Vector2 a, Vector2 b, Vector3 sides)
    {
        float offset = Sensing.PolarAngle(b - a) * Mathf.Deg2Rad;
        float angle = offset - Mathf.Acos((sides.x * sides.x + sides.y * sides.y - sides.z * sides.z) / (2 * sides.x * sides.y));

        b = a + new Vector2(Mathf.Cos(offset) * sides.x,
                                    Mathf.Sin(offset) * sides.x);
        Vector2 c = a + new Vector2(Mathf.Cos(angle) * sides.y,
                                    Mathf.Sin(angle) * sides.y);

        return new List<Vector2> { a, b, c };
    }

    private void LateUpdate()
    {
        /*
        sensor2.IntegrateTouchData(new List<Vector2>
        {
            new Vector2(Random.value * Screen.width, Random.value * Screen.height),
            new Vector2(Random.value * Screen.width, Random.value * Screen.height),
            new Vector2(Random.value * Screen.width, Random.value * Screen.height),
        });
        */
        
        plotData.Clear();

        foreach (var token in sensor2.knowledge.tokens)
        {
            foreach (var data in token.training)
            {
                Color color = Color.HSVToRGB(token.id / 10f, 1, 1);

                plotData.Add(new Data
                {
                    id = token.id,
                    color = color,
                    plot = data,
                });

                plotData.Add(new Data
                {
                    id = token.id,
                    color = color,
                    plot = Triangle.Cycle(data),
                });

                plotData.Add(new Data
                {
                    id = token.id,
                    color = color,
                    plot = Triangle.Cycle(Triangle.Cycle(data)),
                });
            }
        }

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



        if (plotData.Count > 0)
        {
            Vector3 mean = Vector3.zero;

            foreach (var data in currentData)
            {
                mean += data;
            }

            mean /= currentData.Count;

            //debugText.text = string.Format("{0} -> {1}", count, mean);

            /*
            var points = SidesToTriangle(Vector2.zero, mean);
            points.Add(points[0]);
            triangleRenderer.SetPositions(points.Select(point => (Vector3) (point / 2.54f * Screen.dpi)).ToArray());
            */

            if (Input.touchCount == 2)
            {
                Vector2 a = Input.touches[0].position / Screen.dpi * 2.54f;
                Vector2 b = Input.touches[1].position / Screen.dpi * 2.54f;

                var points = CompleteTriangle(a, b, Triangle.CycleToSide(mean, (a - b).magnitude));
                points.Add(points[0]);
                triangleRenderer.SetPositions(points.Select(point => (Vector3) (point - shapeCanvas.rect.size * 0.5f)).ToArray());

                sensor.frame.centroid = (points[0] + points[1] + points[2]) / 3f;
            }
        }
        else
        {
            Vector2 a = new Vector2(0, 0);
            Vector2 b = new Vector2(3, 0);

            var points = CompleteTriangle(a, b, new Vector3(3, 4, 5));
            points.Add(points[0]);
            triangleRenderer.SetPositions(points.Select(point => (Vector3) point).ToArray());
        }

        sensorPlot.gameObject.SetActive(classifyMode);

        if (!explore && !sensor.valid)
        {
            return;
        }
        else if (classifyMode)
        {
            /*
            if (currentData.Count == 0)
            {
                currentData.Enqueue(sensor.feature);
            }
            else
            {
                currentData.Enqueue(Sensing.CycleVectorToMatchOther(sensor.feature, currentData.Last()));
            }

            Vector3 mean = Vector3.zero;

            foreach (var data in currentData)
            {
                mean += data;
            }

            mean /= currentData.Count;

            Vector3 a = mean;
            Vector3 b = Sensing.CycleVectorComponents(a);
            Vector3 c = Sensing.CycleVectorComponents(a);

            var cycles = new List<Vector3> { a, b, c };

            var best = tokens.OrderBy(token => cycles.Select(cycle => Sensing.Compare(token.mean, cycle)).Min()).First();

            int bestid = tokens.IndexOf(best);

            Debug.LogFormat("{0} - {1}", sensor.feature, bestid);
            */

            ///*
            Vector3 feature = sensor.feature;
            var ordered = plotData.OrderBy(plot => (feature - plot.plot).magnitude).ToList();
            var closest = ordered.Take(16).ToList();
            var best = Enumerable.Range(0, 10)
                                 .OrderByDescending(id => closest.Count(plot => plot.id == id))
                                 .First();

            classifications.Add(best);

            best = Enumerable.Range(0, 10)
                             .OrderByDescending(id => classifications.Count(c => c == id))
                             .First();

            //Debug.LogFormat("{0} - {1}", feature, best);

            sensorPlot.transform.localPosition = feature * plotScale;
            sensorPlot.color = Color.HSVToRGB(best / 10f, 1, 1);
            //*/

            return;
        }

        if (!explore)
        {
            if (currentData.Count == 0)
            {
                currentData.Enqueue(sensor.feature);
            }
            else
            {
                currentData.Enqueue(Triangle.CycleToMatch(sensor.feature, currentData.Last()));
            }
            
            if (currentData.Count > 16)
            {
                //currentData.Dequeue();
            }
            
            /*
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
                plot = Triangle.Cycle(sensor.feature),
            });

            plotData.Add(new Data
            {
                id = count,
                color = Color.HSVToRGB(count / 10f, 1, 1),
                plot = Triangle.Cycle(Triangle.Cycle(sensor.feature)),
            });
            */
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
    }
}
