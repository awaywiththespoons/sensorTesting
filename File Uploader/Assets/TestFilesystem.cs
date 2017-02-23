using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;
using System.IO;

public class TestFilesystem : MonoBehaviour 
{
    public Text text;



    private void Start()
    {
        text.text = string.Join("\n", Directory.GetFiles("/storage/emulated/0/Download/"));
    }
}
