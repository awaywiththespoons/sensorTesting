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
    private Sensor sensor;

    [SerializeField]
    private ParticleSystem plotSystem;

    [SerializeField]
    private Text debugText;

    private int count;

    public void Increment()
    {
        count += 1;

        sensor.SetTraining(new Sensor.Token { id = count });
    }

    public void Reset()
    {
        count = 0;

        sensor.Reset();
        sensor.SetTraining(new Sensor.Token { id = count });
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

    private void LateUpdate()
    {
        int count = sensor.allTraining.Count * 3;

        for (int i = 0; i < sensor.allTraining.Count; ++i)
        {
            Sensor.Data data = sensor.allTraining[i];

            Color color = Color.HSVToRGB(data.token.id / 10f, 1, 1);

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
