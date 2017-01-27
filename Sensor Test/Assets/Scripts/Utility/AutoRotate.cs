using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class AutoRotate : MonoBehaviour 
{
    public Vector3 axis;
    public float period;

    private void Update()
    {
        transform.Rotate(axis, 360 * Time.deltaTime / period);
    }
}
