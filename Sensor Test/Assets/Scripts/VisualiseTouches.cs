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

    public float meanType;
    public float meanAngle;
    public float meanLength;

    public Token token;
}

public class VisualiseTouches : MonoBehaviour 
{
	[Header("Simulation Settings")]
	public bool debugOn;
	public Token debugToken;
	[Range(0, 100)]
	public float debugX;
	[Range(0, 100)]
	public float debugY;
	[Range(0, 720)]
	public float debugDirection;

    [Header("Internal Setup")]
    [SerializeField]
    private GameObject tokenCollection;
    public Sensing sensing;
    public Sensor sensor;
    [SerializeField]
    private Image touchPrefab;
    [SerializeField]
    private Image plotPrefab;
    [Range(1, 60)]
    public int requiredDataFrames;
    private List<Token> tokens = new List<Token>();

    public IndexedPool<Image> touchIndicators;
    public IndexedPool<Image> touchIndicators2;
    public IndexedPool<Image> plots;

    private void Awake()
    {
        touchIndicators = new IndexedPool<Image>(touchPrefab);
        touchIndicators2 = new IndexedPool<Image>(touchPrefab);
        plots = new IndexedPool<Image>(plotPrefab);

        tokens = tokenCollection.GetComponentsInChildren<Token>(true).ToList();

        sensor.OnTokenClassified += token => Debug.LogFormat("TOKEN IS {0} ({1})", token.id, tokens[token.id]);
        sensor.OnTokenLifted += () => Debug.Log("REMOVED");

#if UNITY_ANDROID && !UNITY_EDITOR
        debugOn = false;
#endif
    }

    private Context context;

    private List<Vector3> values = new List<Vector3>();

    private void Update()
    {
        int count = Input.touchCount;

        touchIndicators.SetActive(count);
        touchIndicators2.SetActive(count);

        for (int i = 0; i < count; ++i)
        {
            var touch = Input.GetTouch(i);

            touchIndicators[i].transform.position = touch.position;
            touchIndicators2[i].transform.position = touch.position * 0.2f;
            touchIndicators2[i].transform.localScale = Vector3.one * 0.2f;
        }

		if (debugOn) {
			context = new Context {
				token = debugToken,
			};

			sensing.position = new Vector2(debugX, debugY) * 10f;
			sensing.angle = debugDirection;
		}

        if (sensor.detected != null)
        {
            for (int i = 0; i < tokens.Count; ++i)
            {
                var token = tokens[i];

                if (i == sensor.detected.id)
                {
                    token.Refresh(sensor.history.Last().position, sensing.angle);
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
