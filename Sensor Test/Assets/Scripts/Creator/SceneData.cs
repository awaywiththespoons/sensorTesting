using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

namespace Model
{
    [Serializable]
    public class Image
    {
        public bool ghost;
        public string name;
        public string path;

        //[NonSerialized]
        public Sprite sprite;

        public List<Vector2> positions = new List<Vector2> { Vector2.zero };
        public List<float> directions = new List<float> { 0 };
        public List<float> scales = new List<float> { 1 };

        public int frames
        {
            get
            {
                return Mathf.Min(positions.Count, directions.Count, scales.Count);
            }
        }
    }

    [Serializable]
    public class Scene
    {
        public List<Image> images;
    }

    [Serializable]
    public class Story
    {
        public List<Scene> scenes = new List<Scene>();
    }
}
