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

    private Sensor sensor;
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

    private int count;

    public void Increment()
    {
        count += 1;

        sensor.SetTraining(new Sensor.Token { id = count });
    }

    public void Reset()
    {
        count = 0;
        plotData.Clear();
        plots.SetActive(0);

        sensor.Reset();
        sensor.SetTraining(new Sensor.Token { id = count });
    }

    public void SetClassify(bool classify)
    {
        sensor.SetClassify();
    }

    private void Awake()
    {
        plots = new IndexedPool<SpriteRenderer>(plotTemplate);

        Reset();
    }

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
        plotData.Clear();

        foreach (var token in sensor.knowledge.tokens)
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
    }
}
