﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class VisualiseTouches : MonoBehaviour 
{
    [SerializeField]
    private GameObject debug;

    [Header("Settings")]
    public bool stickyTokens;
    public float inactivityThreshold;
    public float inactivityFadeInSpeed;
    public float inactivityFadeOutSpeed;
    public float triggerFadeTime;

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
    public RectTransform regionMatchingObject;
    [SerializeField]
    private GameObject tokenCollection;
    public Sensor sensor;
    [SerializeField]
    private Image touchPrefab;
    private List<Token> tokens = new List<Token>();
    [SerializeField]
    private CanvasGroup inactivitySplash;
    private float inactivityTime;
    private float inactivityFadeVelocity;

    public IndexedPool<Image> touchIndicators;
    public IndexedPool<Image> touchIndicators2;

    private void Awake()
    {
        touchIndicators = new IndexedPool<Image>(touchPrefab);
        touchIndicators2 = new IndexedPool<Image>(touchPrefab);

        tokens = tokenCollection.GetComponentsInChildren<Token>(true).ToList();

        foreach (var token in tokens)
        {
            token.gameObject.SetActive(true);
        }

        sensor.OnTokenClassified += token => Debug.LogFormat("TOKEN IS {0} ({1})", token.id, tokens[token.id]);
        sensor.OnTokenLifted += () => Debug.Log("REMOVED");
        //sensor.OnTokenTracked += frame => { arrow.anchoredPosition = frame.position; arrow.localEulerAngles = Vector3.forward * frame.direction; };

#if UNITY_ANDROID && !UNITY_EDITOR
        debugOn = false;
#endif
    }

    private void Start()
    {
        foreach (var token in tokens)
        {
            token.gameObject.SetActive(false);
        }
    }

    private List<Vector3> values = new List<Vector3>();

    private void Update()
    {
        int count = Input.touchCount;

        if (count == 0 && !debugOn)
        {
            inactivityTime += Time.deltaTime;
        }
        else
        {
            inactivityTime = 0;
        }

        bool inactive = inactivityTime >= inactivityThreshold;

        inactivitySplash.alpha = Mathf.SmoothDamp(inactivitySplash.alpha, 
                                                  inactive ? 1 : 0, 
                                                  ref inactivityFadeVelocity, 
                                                  inactive ? inactivityFadeInSpeed : inactivityFadeOutSpeed);

        touchIndicators.SetActive(count);
        touchIndicators2.SetActive(count);

        for (int i = 0; i < count; ++i)
        {
            var touch = Input.GetTouch(i);

            touchIndicators[i].transform.position = touch.position;
            touchIndicators2[i].transform.position = touch.position * 0.2f;
            touchIndicators2[i].transform.localScale = Vector3.one * 0.2f;
        }

        tokenCollection.SetActive(!debug.activeInHierarchy);

        if (sensor.detected != null || debugOn)
        {
            int id = debugOn ? tokens.IndexOf(debugToken) : sensor.detected.id;
            var frame = debugOn ? new Sensor.Frame() : sensor.history.Last();

            if (debugOn)
            {
                frame.position = new Vector2(debugX, debugY);
                frame.direction = debugDirection;
            }

            for (int i = 0; i < tokens.Count; ++i)
            {
                var token = tokens[i];

                if (i == id)
                {
                    token.Refresh(frame.position, frame.direction + sensor.detected.directionOffset);
                }
                else
                {
                    token.Disable();
                }
            }
        }
        else if (!stickyTokens)
        {
            for (int i = 0; i < tokens.Count; ++i)
            {
                var token = tokens[i];

                token.Disable();
            }
        }
    }
}
