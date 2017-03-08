using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;
using Path = System.IO.Path;

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
    private GameObject storyBrowsePanel;
    [SerializeField]
    private GameObject tokenBrowsePanel;
    [SerializeField]
    private GameObject sceneCreatorHUD;
    [SerializeField]
    private InstancePoolSetup storiesSetup;
    private InstancePool<string> stories;
    [SerializeField]
    private InstancePoolSetup scenesSetup;
    private InstancePool<int> scenes;

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

    public Model.Scene editScene;
    public Model.Story story;

    [SerializeField]
    private GameObject toolbarObject;

    private List<ImageResource> imageResources = new List<ImageResource>();

    public bool replaceMode;
    public bool playMode;

    public IEnumerable<string> GetStories()
    {
        string path = string.Format("{0}/stories/", 
                                    Application.persistentDataPath);

        return System.IO.Directory.GetFiles(path).Select(file => Path.GetFileNameWithoutExtension(file));
    }

    public void OpenScene(int scene)
    {
        OpenScene(story.scenes[scene]);

        tokenBrowsePanel.SetActive(false);
        sceneCreatorHUD.SetActive(true);
    }

    public void OpenScene(Model.Scene scene)
    {
        editScene = scene;

        this.scene.SetConfig(scene);
        this.scene.Refresh();
    }

    public void OpenStory(string name)
    {
        System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/stories/");

        storyBrowsePanel.SetActive(false);

        string path = string.Format("{0}/stories/{1}.json", 
                                    Application.persistentDataPath, 
                                    name);

        string data = System.IO.File.ReadAllText(path);
        var story = JsonUtility.FromJson<Model.Story>(data);
        story.name = name;
        
        this.story = story;

        foreach (var scene in story.scenes)
        {
            foreach (var image in scene.images)
            {
                image.sprite = imageResources.Find(r => r.name == image.path).sprite;
            }
        }

        for (int i = story.scenes.Count; i < 9; ++i)
        {
            story.scenes.Add(new Model.Scene
            {
                frameCount = 15,
                images = new List<Model.Image>(),
            });
        }

        scenes.SetActive(Enumerable.Range(0, 9).ToList());
        tokenBrowsePanel.SetActive(true);
    }

    public void SaveStory(Model.Story story)
    {
        System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/stories/");

        string path = string.Format("{0}/stories/{1}.json", 
                                    Application.persistentDataPath, 
                                    story.name);

        System.IO.File.WriteAllText(path, JsonUtility.ToJson(story));
    }

    public void SaveScene()
    { 
        SaveStory(story);
    }

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

        editScene.images.Add(graphic);

        var screen = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);

        float scale = screen.x / graphic.sprite.rect.width * 0.75f;

        graphic.positions[0] = screen * 0.5f;
        graphic.scales[0] = scale;
        graphic.SetFrameCount(editScene.frameCount);

        selectedImage = graphic;

        scene.Refresh();
    }

    private IEnumerator Start()
    {
        if (GetStories().Count() == 0)
        {
            // TODO: create blank storiess
        }

        scenes = scenesSetup.Finalise<int>();
        stories = storiesSetup.Finalise<string>();
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

        editScene.SetFrameCount(15);
        scene.SetConfig(editScene);

        timelineSlider.maxValue = editScene.frameCount;

        positionDrag.OnBegin += OnPositionDragBegin;
        positionDrag.OnDisplacementChanged += OnPositionDragChange;

        rotationDrag.OnBegin += OnRotationDragBegin;
        rotationDrag.OnDisplacementChanged += OnRotationDragChange;

        scalingDrag.OnBegin += OnScalingDragBegin;
        scalingDrag.OnDisplacementChanged += OnScalingDragChange;

        layerDrag.OnBegin += OnLayerDragBegin;
        layerDrag.OnDisplacementChanged += OnLayerDragChange;

        stories.SetActive(GetStories());
    }
    
    private Vector2 initialPosition;
    private float initialRotation;
    private float initialScaling;
    private int initialLayer;

    private Model.Image selectedImage;

    private int GetFrame(int offset = 0)
    {
        int count = editScene.frameCount;
        int frame = Mathf.CeilToInt(timelineSlider.value)
                  + count
                  + offset;

        frame %= count;

        return frame;
    }

    private void OnPositionDragBegin()
    {
        int frame = GetFrame();
        
        initialPosition = selectedImage.positions[frame];
    }

    private void OnPositionDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        selectedImage.positions[frame] = initialPosition + displacement;
    }

    private void OnRotationDragBegin()
    {
        int frame = GetFrame();

        initialRotation = selectedImage.directions[frame];
    }

    private void OnRotationDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        selectedImage.directions[frame] = initialRotation + displacement.y * 0.001f * 360;
    }

    private void OnScalingDragBegin()
    {
        int frame = GetFrame();

        initialScaling = selectedImage.scales[frame];
    }

    private void OnScalingDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        selectedImage.scales[frame] = initialScaling * Mathf.Pow(4, displacement.y * 0.001f);
    }

    private void OnLayerDragBegin()
    {
        int frame = GetFrame();

        initialLayer = editScene.images.IndexOf(selectedImage);
    }

    private void OnLayerDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        //initialScaling * Mathf.Pow(4, displacement.y * 0.001f);

        int offset = Mathf.FloorToInt(displacement.y * 0.01f);

        int nextLayer = Mathf.Clamp(initialLayer + offset, 0, editScene.images.Count - 1);

        editScene.images.Remove(selectedImage);
        editScene.images.Insert(nextLayer, selectedImage);
        scene.Refresh();
    }

    public void StepBack()
    {
        timelineSlider.value = Mathf.CeilToInt(timelineSlider.value + editScene.frameCount - 1) % editScene.frameCount;
    }

    public void StepForward()
    {
        timelineSlider.value = Mathf.FloorToInt(timelineSlider.value + 1) % editScene.frameCount;
    }

    private List<RaycastResult> raycasts = new List<RaycastResult>();

    private float fadeVelocity;

    public void RemoveSelected()
    {
        toolbarObject.SetActive(false);

        editScene.images.Remove(selectedImage);
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

    public void PreviewScene()
    {
        PlayScene(editScene);
    }

    public void StopPreview()
    {
        sceneCreatorHUD.SetActive(true);
        playMode = false;
    }

    public void PlayScene(Model.Scene scene)
    {
        OpenScene(scene);

        sceneCreatorHUD.SetActive(false);

        playMode = true;
    }

    private void Update()
    {
        if (scene.config == null)
            return;

        if (playMode)
        {
            float time = timelineSlider.value;
            time += Time.deltaTime * 10;
            time %= timelineSlider.maxValue;
            timelineSlider.value = time;
        }

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
            StopPreview();

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
            else if (images.Count > 0)
            {
                selectedImage = images[0];
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
