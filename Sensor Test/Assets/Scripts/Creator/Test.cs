using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

using UnityEngine.EventSystems;

public class Test : MonoBehaviour 
{
    [SerializeField]
    private InstancePoolSetup imagesSetup;
    private InstancePool<ImageResource> images;

    public class ImageResource
    {
        public string name;
        public Texture2D texture;
        public Sprite sprite;
    }

    [SerializeField]
    private GameObject loadingScreen;
    [SerializeField]
    private Slider loadingSlider;

    [SerializeField]
    private DragListener positionDrag;
    [SerializeField]
    private DragListener rotationDrag;
    [SerializeField]
    private DragListener scalingDrag;

    [SerializeField]
    private CanvasGroup fadeGroup;

    [SerializeField]
    private GraphicRaycaster raycaster;
    [SerializeField]
    private GraphicRaycaster sceneRaycaster;

    [SerializeField]
    private Slider timelineSlider;

    [SerializeField]
    private Canvas sceneCanvas;

    [SerializeField]
    private SceneView scene;

    [SerializeField]
    private Model.Scene data;

    [SerializeField]
    private GameObject toolbarObject;

    private List<ImageResource> imageResources = new List<ImageResource>();

    public void AddImageResource(ImageResource resource)
    {
        var graphic = new Model.Image
        {
            path = resource.name,
            sprite = resource.sprite,
            name = resource.name,
        };

        data.images.Add(graphic);

        graphic.positions[0] = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight) * 0.5f;
        graphic.SetFrameCount(data.frameCount);

        selected = data.images.Count - 1;

        scene.Refresh();
    }

    private IEnumerator Start()
    {
        images = imagesSetup.Finalise<ImageResource>();

        loadingScreen.SetActive(true);

        string root = "/storage/emulated/0/Download/Creator Images";

#if UNITY_EDITOR
        root = @"C:\Users\mark\Documents\BUILDS\flipology-uploader\files to upload\mark";
#endif

        System.IO.Directory.CreateDirectory(root);

        loadingSlider.value = 0;
        loadingSlider.maxValue = 0;

        foreach (string file in System.IO.Directory.GetFiles(root))
        {
            var texture = new Texture2D(1, 1);

            string name = System.IO.Path.GetFileNameWithoutExtension(file);

            loadingSlider.maxValue += 1;

            //ThreadedJob.Run<ThreadedReadBytes>(read => read.path = file,
                                               //read =>
            {
                //texture.LoadImage(read.data, true);
                texture.LoadImage(System.IO.File.ReadAllBytes(file), true);

                imageResources.Add(new ImageResource
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

                loadingSlider.value += 1;
            }//);

            yield return null;
        }

        loadingScreen.SetActive(false);
        images.SetActive(imageResources);

        yield return null;

        foreach (var image in data.images)
        {
            image.sprite = imageResources[Random.Range(0, imageResources.Count)].sprite;
        }

        data.SetFrameCount(30);
        scene.SetConfig(data);

        timelineSlider.maxValue = data.frameCount;

        positionDrag.OnBegin += OnPositionDragBegin;
        positionDrag.OnDisplacementChanged += OnPositionDragChange;
        positionDrag.OnEnd += OnPositionDragEnd;

        rotationDrag.OnBegin += OnRotationDragBegin;
        rotationDrag.OnDisplacementChanged += OnRotationDragChange;
        rotationDrag.OnEnd += OnRotationDragEnd;

        scalingDrag.OnBegin += OnScalingDragBegin;
        scalingDrag.OnDisplacementChanged += OnScalingDragChange;
        scalingDrag.OnEnd += OnScalingDragEnd;
    }

    private bool draggingPosition;
    private Vector2 initialPosition;

    private bool draggingRotation;
    private float initialRotation;

    private bool draggingScaling;
    private float initialScaling;

    private int GetFrame(int offset = 0)
    {
        if (selected == -1)
        {
            return data.images[0].frameCount;
        }

        int frame = Mathf.CeilToInt(timelineSlider.value)
                  + data.images[selected].frameCount
                  + offset;

        frame %= data.images[selected].frameCount;

        return frame;
    }

    private void OnPositionDragBegin()
    {
        int frame = GetFrame();

        draggingPosition = true;
        initialPosition = data.images[selected].positions[frame];
    }

    private void OnPositionDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        data.images[selected].positions[frame] = initialPosition + displacement;
    }

    private void OnPositionDragEnd()
    {
        draggingPosition = false;
    }

    private void OnRotationDragBegin()
    {
        int frame = GetFrame();

        draggingRotation = true;
        initialRotation = data.images[selected].directions[frame];
    }

    private void OnRotationDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        data.images[selected].directions[frame] = initialRotation + displacement.y * 0.001f * 360;
    }

    private void OnRotationDragEnd()
    {
        draggingRotation = false;
    }

    private void OnScalingDragBegin()
    {
        int frame = GetFrame();

        draggingScaling = true;
        initialScaling = data.images[selected].scales[frame];
    }

    private void OnScalingDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        data.images[selected].scales[frame] = initialScaling * Mathf.Pow(4, displacement.y * 0.001f);
    }

    private void OnScalingDragEnd()
    {
        draggingScaling = false;
    }

    private int fps = 10;

    public void StepBack()
    {
        timelineSlider.value = Mathf.CeilToInt(timelineSlider.value + data.frameCount - 1) % data.frameCount;
    }

    public void StepForward()
    {
        timelineSlider.value = Mathf.FloorToInt(timelineSlider.value + 1) % data.frameCount;
    }

    private List<RaycastResult> raycasts = new List<RaycastResult>();

    private float fadeVelocity;

    int selected = -1;
    
    public void RemoveSelected()
    {
        toolbarObject.SetActive(false);

        data.images.RemoveAt(selected);
        scene.Refresh();
        selected = -1;
    }

    public void CopyForwardSelected()
    {
        int prev = GetFrame();
        int next = GetFrame(1);

        var image = data.images[selected];

        image.positions[next] = image.positions[prev];
        image.directions[next] = image.directions[prev];
        image.scales[next] = image.scales[prev];

        timelineSlider.value = next;
    }

    private void Update()
    {
        if (scene.config == null)
            return;

        if (positionDrag.dragging || rotationDrag.dragging || scalingDrag.dragging)
        {
            fadeGroup.alpha = Mathf.SmoothDamp(fadeGroup.alpha, 0.01f, ref fadeVelocity, 0.1f);
        }
        else
        {
            fadeGroup.alpha = Mathf.SmoothDamp(fadeGroup.alpha, 1f, ref fadeVelocity, 0.1f);
        }

        var pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        raycasts.Clear();
        raycaster.Raycast(pointer, raycasts);
        bool valid = raycasts.Count == 0;

        int frame = GetFrame();

        if (Input.GetMouseButtonDown(0) && valid)
        {
            raycasts.Clear();
            sceneRaycaster.Raycast(pointer, raycasts);

            var images = raycasts.Select(cast => cast.gameObject.GetComponent<ImageView>())
                                 .OfType<ImageView>()
                                 .ToList();

            bool selectionChanged = false;

            if (selected >= 0)
            { 
                for (int i = 0; i < images.Count - 1; ++i)
                {
                    var view = images[i];

                    if (data.images[selected] == view.config)
                    {
                        selected = data.images.IndexOf(images[i + 1].config);
                        selectionChanged = true;
                    }
                }
            }

            if (!selectionChanged && images.Count > 0)
            {
                selected = data.images.IndexOf(images[0].config);
            }

            toolbarObject.SetActive(true);
        }

        scene.images.MapActive((i, view) =>
        {
            view.selected = (i == selected);
        });

        scene.SetFrame(timelineSlider.value);
    }
}
