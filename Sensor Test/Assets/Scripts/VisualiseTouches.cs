﻿using UnityEngine;
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

    public IndexedPool<Image> touchIndicators;
    public IndexedPool<Image> touchIndicators2;
    public IndexedPool<Image> plots;
   
    private void Awake()
    {
        touchIndicators = new IndexedPool<Image>(touchPrefab);
        touchIndicators2 = new IndexedPool<Image>(touchPrefab);
    }

    private Context context;

    private void Update()
    {
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

            Debug.Log("Token has been removed");
        }
        
        if (count == 3)
        {
            var sorted = Sensing.SortVectorComponents(sensing.feature);

            Debug.LogFormat("feature: {0:0.0}, {1:0.0}, {2:0.0}", 
                            sorted.x, 
                            sorted.y, 
                            sorted.z);
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
