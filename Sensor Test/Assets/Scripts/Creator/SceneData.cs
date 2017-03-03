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
    public partial class Image
    {
        public bool ghost;
        public string name;
        public string path;

        //[NonSerialized]
        public Sprite sprite;

        public int frameCount;
        public List<Vector2> positions = new List<Vector2> { Vector2.zero };
        public List<float> directions = new List<float> { 0 };
        public List<float> scales = new List<float> { 1 };
    }

    public partial class Image
    {
        public void SetFrameCount(int frames)
        {
            frameCount = frames;
            positions.SetLength(frames);
            directions.SetLength(frames);
            scales.SetLength(frames);
        }
    }

    public static partial class Extensions
    {
        public static void SetLength<T>(this IList<T> list, int length)
        {
            T value = default(T);

            if (list.Count > 0)
            {
                value = list[list.Count - 1];
            }

            while (list.Count > length)
            {
                list.RemoveAt(list.Count - 1);
            }

            while (list.Count < length)
            {
                list.Add(value);
            }
        }
    }

    [Serializable]
    public partial class Scene
    {
        public int frameCount = 5;
        public List<Image> images;
    }

    public partial class Scene
    {
        public void SetFrameCount(int frames)
        {
            frameCount = frames;

            for (int i = 0; i < images.Count; ++i)
            {
                images[i].SetFrameCount(frames);
            }
        }
    }

    [Serializable]
    public class Story
    {
        public List<Scene> scenes = new List<Scene>();
    }
}
