﻿using UnityEngine;
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
    public class KeyFrame
    {
        public static KeyFrame Copy(KeyFrame original)
        {
            return new KeyFrame
            {
                position = original.position,
                direction = original.direction,
                scale = original.scale,
            };
        }

        public Vector2 position;
        public float direction;
        public float scale;
    }

    [Serializable]
    public partial class Image
    {
        public bool ghost;
        public string name;
        public string path;

        [NonSerialized]
        public Sprite sprite;

        // obsolete
        public int frameCount;
        public List<Vector2> positions = new List<Vector2> { Vector2.zero };
        public List<float> directions = new List<float> { 0 };
        public List<float> scales = new List<float> { 1 };
        //

        public List<KeyFrame> keyframes = new List<KeyFrame>();
    }

    public partial class Image
    {
        public void SetFrameCount(int frames)
        {
            KeyFrame template;

            if (keyframes.Count > 0)
            {
                template = keyframes.Last();
            }
            else
            {
                template = new KeyFrame
                {
                    position = Vector2.zero,
                    direction = 0,
                    scale = 1,
                };
            }

            while (keyframes.Count > frames)
            {
                keyframes.RemoveAt(keyframes.Count - 1);
            }

            while (keyframes.Count < frames)
            {
                keyframes.Add(KeyFrame.Copy(template));
            }
        }
    }

    [Serializable]
    public partial class Scene
    {
        public string name;
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
        public string name;
        public List<Scene> scenes = new List<Scene>();
    }
}
