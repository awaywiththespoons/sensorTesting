using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class Hotspot : MonoBehaviour 
{
	[Header("Conditions")]
    public RectTransform activeArea;
    [Range(0, 360)]
    public float angleTarget;
    [Range(10, 180)]
    public float angleMargin;

	[Header("Animation")]
    public GameObject graphic;
    public GameObject scene;

    public bool IsValid(Vector2 position, float angle)
    {
        bool inActiveArea = activeArea == null 
                         || RectTransformUtility.RectangleContainsScreenPoint(activeArea, position);
        bool inAngleRange = Mathf.Abs(Mathf.DeltaAngle(angle, angleTarget)) <= angleMargin;

		Debug.LogFormat ("{0} - {1}/{2}", name, inActiveArea, inAngleRange);

        return inAngleRange && inActiveArea;
    }

    public void Refresh(Vector2 position, float angle)
    {
        graphic.SetActive(true);
        scene.SetActive(true);

        graphic.transform.eulerAngles = Vector3.forward * angle;
        graphic.transform.position = position;
    }

    public void Disable()
    {
        graphic.SetActive(false);
        scene.SetActive(false);
    }

    public void SetActive(bool active)
    {
        graphic.SetActive(active);
        scene.SetActive(active);
    }
}
