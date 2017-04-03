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
    [SerializeField]
    private Sensor sensor;

    [SerializeField]
    private ParticleSystem plotSystem;

    [SerializeField]
    private Transform rotate;

    [SerializeField]
    private Text activeIDText;
    [SerializeField]
    private Toggle trainToggle;
    [SerializeField]
    private Slider directionOffsetSlider;

    private int highlight;

    public void Prev()
    {
        highlight = Mathf.Max(highlight - 1, 0);
        sensor.SetClassify();
    }

    public void Next()
    {
        highlight = Math.Min(highlight + 1, 8);
        sensor.SetClassify();
    }

    public void Clear()
    {
        sensor.knowledge.tokens[highlight].training.Clear();
        sensor.allTraining.RemoveAll(data => data.token.id == highlight);
        sensor.SetClassify();
    }

    public void Train()
    {
        sensor.SetTraining(sensor.knowledge.tokens[highlight]);
    }

    public void SetClassify(bool classify)
    {
        sensor.SetClassify();
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
        float offset = Sensor.PolarAngle(b - a) * Mathf.Deg2Rad;

        float angle = offset - Mathf.Acos((sides.x * sides.x + sides.y * sides.y - sides.z * sides.z) / (2 * sides.x * sides.y));

        b = a + new Vector2(Mathf.Cos(offset) * sides.x,
                                    Mathf.Sin(offset) * sides.x);
        Vector2 c = a + new Vector2(Mathf.Cos(angle) * sides.y,
                                    Mathf.Sin(angle) * sides.y);

        return new List<Vector2> { a, b, c };
    }

    private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[4096];

    private void Awake()
    {
        trainToggle.onValueChanged.AddListener(active =>
        {
            if (active)
            {
                sensor.SetTraining(sensor.knowledge.tokens[highlight]);
            }
            else
            {
                sensor.SetClassify();
            }
        });
    }

    private void LateUpdate()
    {
        rotate.Rotate(Vector3.up, Time.deltaTime * 45);

        for (int i = sensor.knowledge.tokens.Count; i < 8; ++i)
        {
            sensor.knowledge.tokens.Add(new Sensor.Token
            {
                id = i,
            });
        }

        //sensor.knowledge.tokens[highlight].directionOffset = directionOffsetSlider.value * 180;

        if (trainToggle.isOn && sensor.training == null)
        {
            trainToggle.isOn = false;
        }

        activeIDText.text = (highlight + 1).ToString();

        int count = sensor.allTraining.Count * 3;

        while (count > particles.Length)
        {
            particles = new ParticleSystem.Particle[particles.Length * 2];
        }

        for (int i = 0; i < sensor.allTraining.Count; ++i)
        {
            Sensor.Data data = sensor.allTraining[i];

            Color color = Color.HSVToRGB(data.token.id / 10f, 1, 1);

            if (data.token.id == highlight)
            {
                color = Color.HSVToRGB(data.token.id / 10f, 1, (Mathf.Sin(Time.realtimeSinceStartup * 10f) * 0.5f + 0.5f));
            }

            Vector3 a = data.feature;
            Vector3 b = Triangle.Cycle(a);
            Vector3 c = Triangle.Cycle(b);

            particles[i * 3 + 0].position = a;
            particles[i * 3 + 1].position = b;
            particles[i * 3 + 2].position = c;

            particles[i * 3 + 0].startColor = color;
            particles[i * 3 + 1].startColor = color;
            particles[i * 3 + 2].startColor = color;

            particles[i * 3 + 0].startSize = 0.05f;
            particles[i * 3 + 1].startSize = 0.05f;
            particles[i * 3 + 2].startSize = 0.05f;
        }

        float plotScale = 1;

        if (sensor.allTraining.Count > 0)
        {
            float max = sensor.allTraining.Select(data => Mathf.Max(data.feature.x, data.feature.y, data.feature.z)).Max();
            
            if (max > 0)
            {
                plotScale = 1 / max;
            }
        }

        for (int i = 0; i < count; ++i)
        {
            particles[i].position *= plotScale;
        }

        plotSystem.SetParticles(particles, count);
    }
}
