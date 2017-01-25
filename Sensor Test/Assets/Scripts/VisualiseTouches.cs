using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class Context
{
}

public class VisualiseTouches : MonoBehaviour 
{
    [Serializable]
    public class Entry
    {
        [UnityEngine.Serialization.FormerlySerializedAs("angle")]
        public float variable;
        public Sensing.Type type;
        public GameObject container;
        public Transform anchor;
    }

    public Sensing sensing;

    public Camera camera;

    public List<Entry> entries;

    [SerializeField]
    private Image touchPrefab;
    [SerializeField]
    private Text debugText;

    public IndexedPool<Image> touchIndicators;
    public IndexedPool<Image> touchIndicators2;

    public Transform container;

    private void Awake()
    {
        touchIndicators = new IndexedPool<Image>(touchPrefab, transform);
        touchIndicators2 = new IndexedPool<Image>(touchPrefab, transform);
    }

    private float removeTimeout;

    private Context context;
    private List<string> debugs = new List<string>();

    private float totalAngle = 0;
    private int countAngle = 0;
    private float runningAngle = 0;

    private float ScorePoint(int i, List<Vector2> points)
    {
        return points.Sum(point => Vector2.SqrMagnitude(point - points[i]));
    }

    private void Update()
    {
        int count = Input.touchCount;

        touchIndicators.SetActive(count);
        touchIndicators2.SetActive(count);

        string main = "Debug";



        for (int i = 0; i < count; ++i)
        {
            var touch = Input.GetTouch(i);

            touchIndicators[i].transform.position = touch.position;

            touchIndicators2[i].transform.position = touch.position * 0.2f;
            touchIndicators2[i].transform.localScale = Vector3.one * 0.2f;
        }

        /*
        try
        {
            if (count == 3)
            {
                var points = Enumerable.Range(0, 3).Select(i => Input.GetTouch(i).position).ToList();
                var ordered = Enumerable.Range(0, 3)
                                        .OrderBy(i => ScorePoint(i, points))
                                        .Select(i => points[i])
                                        .ToList();

                float p12s = Vector2.SqrMagnitude(ordered[1] - ordered[0]);
                float p13s = Vector2.SqrMagnitude(ordered[2] - ordered[0]);
                float p23s = Vector2.SqrMagnitude(ordered[2] - ordered[1]);

                float angle = Mathf.Acos((p12s + p13s - p23s) / (2 * Mathf.Sqrt(p12s) * Mathf.Sqrt(p13s)));

                countAngle += 1;
                totalAngle += angle;
                runningAngle = Mathf.Lerp(runningAngle, angle, 0.25f);

                var middle = Vector2.Lerp(ordered[1], ordered[2], 0.5f);
                var forward = ordered[0] - middle;
                var rotation = Quaternion.LookRotation(forward, Vector3.back);

                float ang = Mathf.Atan2(forward.y, forward.x);

                container.transform.position = ordered[0];
                container.transform.eulerAngles = Vector3.forward * (ang * Mathf.Rad2Deg - 90);
            }
        }
        catch (Exception e)
        {
            debugs.Add(e.ToString());
        }
        */

        container.transform.position = sensing.position;
        container.transform.eulerAngles = Vector3.forward * (sensing.angle - 90);

        float avg = runningAngle;//totalAngle / countAngle;

        if (context != null)
        {
            var closest = entries.Where(entry => entry.type == sensing.type)
                                 .OrderBy(entry => Math.Abs(entry.variable - sensing.variable))
                                 .First();
            
            for (int i = 0; i < entries.Count; ++i)
            {
                var entry = entries[i];

                if (entry != closest)
                {
                    entry.container.SetActive(false);
                }
                else
                {
                    entry.container.SetActive(true);
                    container = entry.anchor;
                }
            }
            
        }

        main = string.Format("Angle: {0:0.0}", avg * Mathf.Rad2Deg);

        if (count < 2 && context != null)
        {
            removeTimeout += Time.deltaTime;
        }
        else if (count >= 2 && context == null)
        {
            debugs.Add("Placed");
            removeTimeout = 0;
            context = new Context();

            totalAngle = 0;
            countAngle = 0;
            runningAngle = 0;

        }
        else
        {
            removeTimeout = 0;
        }

        if (removeTimeout > .5f)
        {
            debugs.Add("Removed");
            context = null;
            camera.backgroundColor = Color.black;

            for (int i = 0; i < entries.Count; ++i)
            {
                var entry = entries[i];

                entry.container.SetActive(false);
            }
        }
        debugText.text = string.Format("{0}\n{1}", 
                                main,
                                string.Join("\n", debugs.Reverse<string>().Take(3).Reverse<string>().ToArray()));

    }
}
