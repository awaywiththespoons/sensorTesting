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
        public bool hide = false;
    }

    [Serializable]
    public partial class Image
    {
        public bool ghost;
        public string name;
        public string path;
        public bool text;

        [NonSerialized]
        public Sprite sprite;

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
    public class SoundFrame
    {
        public List<string> sounds = new List<string>();
    }

    [Serializable]
    public partial class Scene
    {
        public int index;
        public string name;
        public int frameCount = 5;
        public List<Image> images;
        public List<SoundFrame> sounds = new List<SoundFrame>();
        public string bgloop;
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

            while (sounds.Count > frames)
            {
                sounds.RemoveAt(sounds.Count - 1);
            }

            while (sounds.Count < frames)
            {
                sounds.Add(new SoundFrame());
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
