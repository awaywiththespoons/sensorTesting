using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class Billboard : MonoBehaviour 
{
    private void Update()
    {
        transform.LookAt(Camera.main.transform.position);
    }
}
