﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class Token : MonoBehaviour 
{
    public Sensing.Type patternType;
    [Tooltip("Triangle: angle of front corner\nLine: length of longest section")]
    public float patternVariableTarget;
    public List<TriggerSettings> triggers = new List<TriggerSettings>();
    //public TriggerSettings defaultHotspot;

    public GameObject ghostObject;

    public void Refresh(Vector2 position, float angle)
    {
        gameObject.SetActive(true);

        if (ghostObject != null)
        {
            ghostObject.SetActive(true);
            ghostObject.transform.eulerAngles = Vector3.forward * angle;
            ghostObject.transform.position = position;
        }

        var hotspot = FindHotspot(position, angle);

        DisableHotspots(hotspot);

        if (hotspot == null)
        {
            return;
        }

        hotspot.Refresh(position, angle);
    }

    private TriggerSettings FindHotspot(Vector2 position, float angle)
    {
        for (int i = 0; i < triggers.Count; ++i)
        {
            var hotspot = triggers[i];

            if (hotspot.IsValid(position, angle))
            {
                return hotspot;
            }
        }

        return null;
        //return defaultHotspot;
    }

    private void DisableHotspots(TriggerSettings except)
    {
        for (int i = 0; i < triggers.Count; ++i)
        {
            if (except != triggers[i])
            {
                triggers[i].Disable();
            }
        }

        /*
        if (except != defaultHotspot)
        {
            defaultHotspot.Disable();
        }
        */
    }

    public void Disable()
    {
        gameObject.SetActive(false);

        if (ghostObject != null)
        {
            ghostObject.SetActive(false);
        }
    }
}
