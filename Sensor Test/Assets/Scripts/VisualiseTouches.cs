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
    public Sensing sensing;
    public List<Token> tokens = new List<Token>();

    [SerializeField]
    private Image touchPrefab;

    public IndexedPool<Image> touchIndicators;
    public IndexedPool<Image> touchIndicators2;

    private void Awake()
    {
        touchIndicators = new IndexedPool<Image>(touchPrefab, transform);
        touchIndicators2 = new IndexedPool<Image>(touchPrefab, transform);
    }

    private float removeTimeout;

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

        if (context != null)
        {
            var closest = tokens.Where(token => token.patternType == sensing.type)
                                .OrderBy(token => Math.Abs(token.patternVariableTarget - sensing.variable))
                                .FirstOrDefault();
            
            if (closest != null)
            {
                Debug.LogFormat("{0}, variable: {1:0} -> {2}", sensing.type, sensing.variable, closest.name);

                for (int i = 0; i < tokens.Count; ++i)
                {
                    var token = tokens[i];

                    if (token == closest)
                    {
                        token.Refresh(sensing.position, sensing.angle);
                    }
                    else
                    {
                        token.Disable();
                    }
                }
            }
        }
        else
        {
            Debug.LogFormat("{0}, variable: {1:0} -> ???", sensing.type, sensing.variable);

            for (int i = 0; i < tokens.Count; ++i)
            {
                var token = tokens[i];

                token.Disable();
            }
        }

        if (count < 2 && context != null)
        {
            removeTimeout += Time.deltaTime;
        }
        else if (count >= 2 && context == null)
        {
            removeTimeout = 0;
            context = new Context();
        }
        else
        {
            removeTimeout = 0;
        }

        if (removeTimeout > .5f)
        {
            context = null;
        }
    }
}
