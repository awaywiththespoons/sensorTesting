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

    [SerializeField]
    private GameObject objectControls;

    public class ImageResource
    {
        public string name;
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
    private DragListener layerDrag;

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

    public bool replaceMode;

    public void SetReplaceMode()
    {
        replaceMode = true;
    }

    public void ReplaceImageResource(ImageResource resource)
    {
        selectedImage.sprite = resource.sprite;
        scene.Refresh();
    }

    public void AddImageResource(ImageResource resource)
    {
        if (replaceMode)
        {
            ReplaceImageResource(resource);
            return;
        }

        var graphic = new Model.Image
        {
            path = resource.name,
            sprite = resource.sprite,
            name = resource.name,
        };

        data.images.Add(graphic);

        var screen = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);

        float scale = screen.x / graphic.sprite.rect.width * 0.75f;

        graphic.positions[0] = screen * 0.5f;
        graphic.scales[0] = scale;
        graphic.SetFrameCount(data.frameCount);

        selectedImage = graphic;

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

        data.SetFrameCount(15);
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

        layerDrag.OnBegin += OnLayerDragBegin;
        layerDrag.OnDisplacementChanged += OnLayerDragChange;
    }

    private bool draggingPosition;
    private Vector2 initialPosition;

    private bool draggingRotation;
    private float initialRotation;

    private bool draggingScaling;
    private float initialScaling;
    private int initialLayer;

    private Model.Image selectedImage;

    private int GetFrame(int offset = 0)
    {
        int count = data.frameCount;
        int frame = Mathf.CeilToInt(timelineSlider.value)
                  + count
                  + offset;

        frame %= count;

        return frame;
    }

    private void OnPositionDragBegin()
    {
        int frame = GetFrame();

        draggingPosition = true;
        initialPosition = selectedImage.positions[frame];
    }

    private void OnPositionDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        selectedImage.positions[frame] = initialPosition + displacement;
    }

    private void OnPositionDragEnd()
    {
        draggingPosition = false;
    }

    private void OnRotationDragBegin()
    {
        int frame = GetFrame();

        draggingRotation = true;
        initialRotation = selectedImage.directions[frame];
    }

    private void OnRotationDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        selectedImage.directions[frame] = initialRotation + displacement.y * 0.001f * 360;
    }

    private void OnRotationDragEnd()
    {
        draggingRotation = false;
    }

    private void OnScalingDragBegin()
    {
        int frame = GetFrame();

        draggingScaling = true;
        initialScaling = selectedImage.scales[frame];
    }

    private void OnScalingDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        selectedImage.scales[frame] = initialScaling * Mathf.Pow(4, displacement.y * 0.001f);
    }

    private void OnScalingDragEnd()
    {
        draggingScaling = false;
    }

    private void OnLayerDragBegin()
    {
        int frame = GetFrame();

        initialLayer = data.images.IndexOf(selectedImage);
    }

    private void OnLayerDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        //initialScaling * Mathf.Pow(4, displacement.y * 0.001f);

        int offset = Mathf.FloorToInt(displacement.y * 0.01f);

        int nextLayer = Mathf.Clamp(initialLayer + offset, 0, data.images.Count - 1);

        data.images.Remove(selectedImage);
        data.images.Insert(nextLayer, selectedImage);
        scene.Refresh();
    }

    private int fps = 5;

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

    public void RemoveSelected()
    {
        toolbarObject.SetActive(false);

        data.images.Remove(selectedImage);
        scene.Refresh();
        selectedImage = null;
    }

    public void CopyForwardSelected()
    {
        int prev = GetFrame();
        int next = GetFrame(1);

        selectedImage.positions[next] = selectedImage.positions[prev];
        selectedImage.directions[next] = selectedImage.directions[prev];
        selectedImage.scales[next] = selectedImage.scales[prev];

        timelineSlider.value = next;
    }

    private void Update()
    {
        if (scene.config == null)
            return;

        objectControls.SetActive(selectedImage != null);

        if (positionDrag.dragging 
         || rotationDrag.dragging 
         || scalingDrag.dragging
         || layerDrag.dragging)
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
                                 .Select(view => view.config)
                                 .ToList();

            if (selectedImage != null && images.Count > 0)
            {
                int index = images.IndexOf(selectedImage);
                int next = 0;

                if (index >= 0)
                {
                    next = (index + 1) % images.Count;
                }

                selectedImage = images[next];
            }
            else
            {
                selectedImage = null;
            }

            toolbarObject.SetActive(true);
        }

        scene.images.MapActive((i, view) =>
        {
            view.selected = (view.config == selectedImage);
        });

        scene.SetFrame(timelineSlider.value);
    }
}
