using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

[RequireComponent(typeof(CanvasGroup))]
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

    private CanvasGroup group;
    private VisualiseTouches control;
    private bool active;
    private float fadeVelocity;

    private Vector3[] worldCorners = new Vector3[4];

    public bool IsValid(Vector2 position, float angle)
    {
        control.regionMatchingObject.anchoredPosition = position;
        moving.transform.position = control.regionMatchingObject.position;

        bool inActiveArea = activeRegion == null
                         || RectTransformUtility.RectangleContainsScreenPoint(activeRegion, control.regionMatchingObject.position);
        bool inAngleRange = Mathf.Abs(Mathf.DeltaAngle(angle, angleDirection)) <= angleRange;

        /*
        activeRegion.GetWorldCorners(worldCorners);

        var rect = Rect.MinMaxRect(worldCorners[0].x, worldCorners[0].y,
                                   worldCorners[2].x, worldCorners[2].y);

        moving.GetComponent<RectTransform>().GetWorldCorners(worldCorners);

        var rect2 = Rect.MinMaxRect(worldCorners[0].x, worldCorners[0].y,
                                    worldCorners[2].x, worldCorners[2].y);

        inActiveArea = rect.Overlaps(rect2);

        Debug.LogFormat(moving, "{0}? ({1} vs {2}) {3}", name, rect, rect2, rect.Overlaps(rect2));
        */

        return inAngleRange && inActiveArea;
    }

    public void Refresh(Vector2 position, float angle)
    {
        active = true;

        moving.transform.eulerAngles = Vector3.forward * angle;
        moving.GetComponent<RectTransform>().anchoredPosition = position;
    }

    public void Disable()
    {
        active = false;
    }

    private void Awake()
    {
        group = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0;

        control = GameObject.FindWithTag("Control").GetComponent<VisualiseTouches>();
    }

    private void Update()
    {
        group.alpha = Mathf.SmoothDamp(group.alpha, active ? 1 : 0, ref fadeVelocity, control.triggerFadeTime);
    }
}
