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
    public int dataFrames;
    public float missingTime;

    public float meanType;
    public float meanAngle;
    public float meanLength;

    public Vector3 meanFeature;

    public Token token;
}

public class VisualiseTouches : MonoBehaviour 
{
    [Range(1, 60)]
    public int requiredDataFrames;

    public Sensing sensing;
    public List<Token> tokens = new List<Token>();

    [SerializeField]
    private Image touchPrefab;
    [SerializeField]
    private Image plotPrefab;

    public IndexedPool<Image> touchIndicators;
    public IndexedPool<Image> touchIndicators2;
    public IndexedPool<Image> plots;

    public LineRenderer tokenPlotPrefab;
    public IndexedPool<LineRenderer> tokenPlots;

    private struct Data
    {
        public Vector3 plot;
        public Color color;
    }

    private int testCount;
    private List<Data> testPlotData = new List<Data>();
    public IndexedPool<LineRenderer> testPlots;

    private void Awake()
    {
        touchIndicators = new IndexedPool<Image>(touchPrefab);
        touchIndicators2 = new IndexedPool<Image>(touchPrefab);
        plots = new IndexedPool<Image>(plotPrefab);

        tokenPlots = new IndexedPool<LineRenderer>(tokenPlotPrefab);
        testPlots = new IndexedPool<LineRenderer>(tokenPlotPrefab);
    }

    private Context context;

    private List<Vector3> values = new List<Vector3>();

    private Vector3 SortVectorComponents(Vector3 vector)
    {
        if (vector.y > vector.z)
        {
            float swap = vector.y;

            vector.y = vector.z;
            vector.z = swap;       
        }
    
        if (vector.x > vector.y)
        {
            float swap = vector.x;

            vector.x = vector.y;
            vector.y = swap;       
        }

        if (vector.y > vector.z)
        {
            float swap = vector.y;

            vector.y = vector.z;
            vector.z = swap;       
        }

        return vector;
    }

    private void Update()
    {
        float plotScale = 0;

        if (testPlotData.Count > 0)
            plotScale = 1 / testPlotData.Select(data => Mathf.Max(data.plot.x, data.plot.y, data.plot.z)).Max();

        Vector3 sortedFeature = SortVectorComponents(context != null ? context.meanFeature : Vector3.zero) * plotScale;

        // possibility - cycle coords to minimise difference between x & y?
        /*
        Vector3 test = new Vector3(Random.value, 0, 0);
        test.y = Mathf.Clamp(Random.value, test.x, 1);
        test.z = Mathf.Clamp(Random.value, test.x, 1);

        testPlotData.Add(new Data { plot = test, color = Color.white });
        */

        testPlots.SetActive(testPlotData.Count);
        testPlots.MapActive((i, plot) =>
        {
            Vector3 sortedTarget = testPlotData[i].plot * plotScale;
            plot.transform.localPosition = sortedTarget;
            plot.GetComponent<SpriteRenderer>().color = testPlotData[i].color;
        });

        tokenPlots.SetActive(tokens.Count + 1);
        tokenPlots.MapActive((i, plot) =>
        {
            if (i == tokens.Count)
            {
                plot.transform.localPosition = sortedFeature;
                plot.transform.localEulerAngles = new Vector3(45, 45, 45);

                plot.enabled = false;
            }
            else
            {
                Vector3 sortedTarget = SortVectorComponents(tokens[i].featureTarget) * plotScale;

                plot.transform.localPosition = sortedTarget;
                plot.transform.localEulerAngles = new Vector3(0, 0, 0);

                float distance = (sortedTarget - sortedFeature).magnitude;
                Color color = Color.white * (1 - Mathf.Clamp01(distance));
                plot.enabled = true;
                plot.startColor = color;
                plot.endColor = color;
            }
        });

        tokenPlots.MapActive((i, plot) =>
        {
            plot.SetPosition(0, plot.transform.position);
            plot.SetPosition(1, tokenPlots[tokens.Count].transform.position);
        });

        int count = Input.touchCount;

        touchIndicators.SetActive(count);
        touchIndicators2.SetActive(count);

        for (int i = 0; i < count; ++i)
        {
            var touch = Input.GetTouch(i);

            touchIndicators[i].transform.position = touch.position;

            if ((touch.position - sensing.position).magnitude < .1f)
            {
                touchIndicators[i].color = Color.red;
            }
            else
            {
                touchIndicators[i].color = Color.white;
            }

            touchIndicators2[i].transform.position = touch.position * 0.2f;
            touchIndicators2[i].transform.localScale = Vector3.one * 0.2f;
        }

        plots.SetActive(values.Count);

        for (int i = 0; i < values.Count; ++i)
        {
            plots[i].transform.position = (Vector2) values[i] + Vector2.up * 200;
            Color color = Color.HSVToRGB(values[i].z, 1, 1);
            color.a = 0.1f;
            plots[i].color = color; 
        }

        if (context == null && count >= 2)
        {
            context = new Context();
            context.meanType = (int) sensing.type;
            context.meanAngle = sensing.variable;
            context.meanLength = sensing.variable;
            context.meanFeature = sensing.feature;
        }
        else if (count < 2 && context != null)
        {
            context.missingTime += Time.deltaTime;
        }

        if (context != null && context.missingTime > .5f)
        {
            context = null;

            Debug.Log("Token has been removed");

            testCount += 1;
        }
        
        if (context != null && context.token == null)
        {
            if (context.dataFrames > requiredDataFrames)
            {
                DecideToken();
            }
            else if (count == 3)
            {
                context.dataFrames += 1;
                context.meanType = Mathf.Lerp(context.meanType, (int) sensing.type, 0.5f);

                context.meanFeature = Vector3.Lerp(context.meanFeature, SortVectorComponents(sensing.feature), 0.25f);

                if (sensing.type == Sensing.Type.Line)
                {
                    context.meanLength = Mathf.Lerp(context.meanLength, sensing.variable, 0.5f);
                }
                else if (sensing.type == Sensing.Type.Triangle)
                {
                    context.meanAngle = Mathf.Lerp(context.meanAngle, sensing.variable, 0.5f);
                }

                Vector3 sorted = SortVectorComponents(context.meanFeature);
                
                testPlotData.Add(new Data { plot = sorted, color = Color.HSVToRGB(testCount / 10f, 1, 1) });

                Debug.LogFormat("feature: {0:0}, {1:0}, {2:0}", 
                                sorted.x, 
                                sorted.y, 
                                sorted.z);
            }
        }

        if (context != null && context.token != null)
        {
            context.meanFeature = Vector3.Lerp(context.meanFeature, SortVectorComponents(sensing.feature), 0.25f);
            Vector3 sorted = SortVectorComponents(context.meanFeature);
            Debug.LogFormat("feature: {0:0}, {1:0}, {2:0}", 
                            sorted.x, 
                            sorted.y, 
                            sorted.z);
            testPlotData.Add(new Data { plot = sorted, color = Color.HSVToRGB(testCount / 10f, 1, 1) });

            for (int i = 0; i < tokens.Count; ++i)
            {
                var token = tokens[i];

                if (token == context.token)
                {
                    token.Refresh(sensing.position, sensing.angle);
                }
                else
                {
                    token.Disable();
                }
            }
        }
        else
        {
            for (int i = 0; i < tokens.Count; ++i)
            {
                var token = tokens[i];

                token.Disable();
            }
        }
    }

    private float BestDistance(Vector3 feature, Vector3 target)
    {
        Vector3 a = new Vector3(target.x, target.y, target.z);
        Vector3 b = new Vector3(target.y, target.z, target.x);
        Vector3 c = new Vector3(target.z, target.x, target.y);

        Vector3 d = new Vector3(target.z, target.y, target.x);
        Vector3 e = new Vector3(target.x, target.z, target.y);
        Vector3 f = new Vector3(target.y, target.x, target.z);

        var variants = new List<Vector3> { a, b, c, d, e, f };

        return variants.Select(variant => (variant - feature).sqrMagnitude).Min();
    }

    private void DecideToken()
    {
        var type = (Sensing.Type) Mathf.RoundToInt(context.meanType);

        float variable = type == Sensing.Type.Line 
                       ? context.meanLength 
                       : context.meanAngle;

        var closest = tokens.Where(token => token.patternType == type)
                            .OrderBy(token => Math.Abs(token.patternVariableTarget - variable))
                            .FirstOrDefault();

        closest = tokens.OrderBy(token => BestDistance(sensing.feature, token.featureTarget)).FirstOrDefault();

        context.token = closest;

        if (closest != null)
        {
            Debug.LogFormat("Decided token with feature: {0:0}, {1:0}, {2:0} ({3:0} distance)",
                            closest.featureTarget.x,
                            closest.featureTarget.y,
                            closest.featureTarget.z,
                            BestDistance(sensing.feature, closest.featureTarget));
        }
    }
}
