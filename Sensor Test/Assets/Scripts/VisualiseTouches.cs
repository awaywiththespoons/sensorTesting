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
    private Image arrowTest;
    [SerializeField]
    private Sensor sensor;

    public IndexedPool<Image> touchIndicators;
    public IndexedPool<Image> touchIndicators2;
    public IndexedPool<Image> plots;
   
    private void Awake()
    {
        touchIndicators = new IndexedPool<Image>(touchPrefab);
        touchIndicators2 = new IndexedPool<Image>(touchPrefab);
    }

    private Context context;

    private void LateUpdate()
    {
        int count = Input.touchCount;

        touchIndicators.SetActive(count);
        touchIndicators2.SetActive(count);

        arrowTest.GetComponent<RectTransform>().anchoredPosition = sensing.frame.centroid;
        arrowTest.transform.eulerAngles = Vector3.forward * sensing.angle;

        for (int i = 0; i < count; ++i)
        {
            Vector2 position = Input.GetTouch(i).position;

            touchIndicators[i].transform.position = position;
            touchIndicators2[i].transform.position = position * 0.2f;
            touchIndicators2[i].transform.localScale = Vector3.one * 0.2f;
        }

        if (context == null && count >= 2)
        {
            context = new Context();
            context.meanFeature = Sensing.SortVectorComponents(sensing.feature);
        }
        else if (count < 2 && context != null)
        {
            context.missingTime += Time.deltaTime;
        }

        if (context != null && context.missingTime > .5f)
        {
            context = null;

            //Debug.Log("Token has been removed");
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
                Vector3 sorted = Sensing.SortVectorComponents(sensing.feature);
            }
        }

        if (context != null && context.token != null)
        {
            context.meanFeature = Vector3.Lerp(context.meanFeature, Sensing.SortVectorComponents(sensing.feature), 1f);
            Vector3 sorted = Sensing.SortVectorComponents(sensing.feature);

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
        Token closest = tokens.OrderBy(token => BestDistance(sensing.feature, token.featureTarget)).FirstOrDefault();

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
