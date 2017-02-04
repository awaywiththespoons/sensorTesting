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
        moving.GetComponent<RectTransform>().anchoredPosition = position;

        bool inActiveArea = activeRegion == null 
                         || RectTransformUtility.RectangleContainsScreenPoint(activeRegion, moving.transform.position);
        bool inAngleRange = Mathf.Abs(Mathf.DeltaAngle(angle, angleDirection)) <= angleRange;

        return inAngleRange && inActiveArea;
    }

    public void Refresh(Vector2 position, float angle)
    {
        moving.SetActive(true);
        @static.SetActive(true);

        moving.transform.eulerAngles = Vector3.forward * angle;
        moving.GetComponent<RectTransform>().anchoredPosition = position;
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
