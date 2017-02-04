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

        sensor.OnTokenPlaced += () => Debug.Log("PLACED");
        sensor.OnTokenLifted += () => Debug.Log("REMOVED");
        sensor.OnTokenClassified += token => Debug.Log("TOKEN IS " + token.id);
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
        
        if (sensor.detected != null)
        { 
            for (int i = 0; i < tokens.Count; ++i)
            {
                var token = tokens[i];

                if (i == sensor.detected.id)
                {
                    token.Refresh(sensor.history.Last().position, 
                                  sensor.history.Last().direction);
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
}
