using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class TriggerSettings : MonoBehaviour 
{
	[Header("Trigger Conditions")]
    public RectTransform activeRegion;
    [Range(0, 360)]
    public float angleDirection;
    [Range(10, 180)]
    public float angleRange;

	[Header("Triggered Context")]
    [Tooltip("")]
    public GameObject moving;
    public GameObject @static;

    public bool IsValid(Vector2 position, float angle)
    {
        bool inActiveArea = activeRegion == null 
                         || RectTransformUtility.RectangleContainsScreenPoint(activeRegion, position);
        bool inAngleRange = Mathf.Abs(Mathf.DeltaAngle(angle, angleDirection)) <= angleRange;

		Debug.LogFormat ("{0} - {1}/{2}", name, inActiveArea, inAngleRange);

        return inAngleRange && inActiveArea;
    }

    public void Refresh(Vector2 position, float angle)
    {
        moving.SetActive(true);
        @static.SetActive(true);

        moving.transform.eulerAngles = Vector3.forward * angle;
        moving.transform.position = position;
    }

    public void Disable()
    {
        moving.SetActive(false);
        @static.SetActive(false);
    }

    public void SetActive(bool active)
    {
        moving.SetActive(active);
        @static.SetActive(active);
    }
}
