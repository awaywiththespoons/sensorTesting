using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class TestImageBrowser : MonoBehaviour 
{
    public class ImageResource
    {
        public string name;
        public Texture2D texture;
        public Sprite sprite;
    }

    [SerializeField]
    private InstancePoolSetup imagesSetup;
    private InstancePool<ImageResource> images;

    private IEnumerator Start()
    {
        images = imagesSetup.Finalise<ImageResource>();

        var resources = new List<ImageResource>();

        string root = "/storage/emulated/0/Download/Creator Images";

#if UNITY_EDITOR
        root = @"C:\Users\mark\Documents\BUILDS\flipology-uploader\files to upload\mark";
#endif

        System.IO.Directory.CreateDirectory(root);

        foreach (string file in System.IO.Directory.GetFiles(root))
        {
            var texture = new Texture2D(1, 1);

            string name = System.IO.Path.GetFileNameWithoutExtension(file);

            ThreadedJob.Run<ThreadedReadBytes>(read => read.path = file,
                                               read =>
            {
                texture.LoadImage(read.data, true);

                resources.Add(new ImageResource
                {
                    name = name,
                    texture = texture,
                    sprite = Sprite.Create(texture, 
                                           Rect.MinMaxRect(0, 0, texture.width, texture.height), 
                                           Vector2.one * 0.5f,
                                           100,
                                           0,
                                           SpriteMeshType.FullRect),
                });

                images.SetActive(resources);
            });

            yield return null;
        }

        images.SetActive(resources);
    }
}
