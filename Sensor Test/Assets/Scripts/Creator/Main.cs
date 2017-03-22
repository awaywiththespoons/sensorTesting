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

public class Main : MonoBehaviour 
{
    public Transform ghostReference;

    [SerializeField]
    private Sensor sensor;

    [SerializeField]
    private InstancePoolSetup imagesSetup;
    private InstancePool<ImageResource> images;

    [SerializeField]
    private InstancePoolSetup soundsSetup;
    private InstancePool<SoundResource> sounds;

    [SerializeField]
    private GameObject objectControls;

    [SerializeField]
    private Sprite clearSprite;

    [SerializeField]
    private Text soundCountText;

    public class ImageResource
    {
        public string name;
        public Sprite sprite;
    }

    public class SoundResource
    {
        public string name;
        public AudioClip sound;
    }

    [SerializeField]
    private GameObject loadingScreen;
    [SerializeField]
    private Slider loadingSlider;

    [SerializeField]
    private Text frameCount;

    [SerializeField]
    private InputField hiddenInputField;

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
    private InstancePool<Model.Scene> scenes;

    [SerializeField]
    private DragListener positionDrag;
    [SerializeField]
    private DragListener rotationDrag;
    [SerializeField]
    private DragListener scalingDrag;
    [SerializeField]
    private DragListener layerDrag;
    [SerializeField]
    private Toggle ghostToggle;

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
    private Dictionary<string, SoundResource> soundResources 
        = new Dictionary<string, SoundResource>();

    public bool replaceMode;
    public bool previewMode;
    public bool playingMode;

    [SerializeField]
    private Canvas menuCanvas;

    public void PlayStory()
    {
        playingMode = true;
        menuCanvas.gameObject.SetActive(false);
        PlayRealScene(-1);
    }

    public void ExitStory()
    {
        playingMode = false;
        menuCanvas.gameObject.SetActive(true);
    }

    public void ClickedAddText()
    {
        if (replaceMode)
        {
            selectedImage.path = "text";
            selectedImage.text = true;
        }
        else
        {
            var graphic = new Model.Image
            {
                path = "some text",
                sprite = null,
                name = "text graphic",
                text = true,
            };

            editScene.images.Add(graphic);

            var screen = new Vector2(Screen.width, Screen.height);

            graphic.keyframes.Add(new Model.KeyFrame
            {
                position = screen * 0.5f,
                scale = 1,
                direction = 0,
            });

            graphic.SetFrameCount(editScene.frameCount);

            selectedImage = graphic;

            scene.Refresh();
        }

        hiddenInputField.onValueChanged.RemoveAllListeners();
        hiddenInputField.onValueChanged.AddListener(text => selectedImage.path = hiddenInputField.text);
        hiddenInputField.Select();
    }

    public IEnumerable<string> GetStories()
    {
        System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/stories/");

        string path = string.Format("{0}/stories/", 
                                    Application.persistentDataPath);

        return System.IO.Directory.GetFiles(path).Select(file => Path.GetFileNameWithoutExtension(file));
    }

    public void OpenEditScene(Model.Scene scene)
    {
        editScene = scene;

        this.scene.SetConfig(scene);
        this.scene.Refresh();

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
                image.sprite = image.text ? clearSprite : imageResources.Find(r => r.name == image.path).sprite;
            }

            // because data may be missing in old save files...
            scene.SetFrameCount(scene.frameCount);
        }

        for (int i = story.scenes.Count; i < 9; ++i)
        {
            var scene = new Model.Scene
            {
                name = "Scene " + (i + 1),
                images = new List<Model.Image>(),
            };

            story.scenes.Add(scene);

            scene.SetFrameCount(10);
        }

        int j = 0;
        foreach (var scene in story.scenes)
        {
            j++;

            if (string.IsNullOrEmpty(scene.name))
            {
                scene.name = "Scene " + j;
            }
        }

        scenes.SetActive(story.scenes);
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

    public void SetAddMode()
    {
        replaceMode = false;
    }

    [SerializeField]
    private AudioSource audioSource;

    public bool FrameContainsSound(SoundResource resource)
    {
        var sounds = editScene.sounds[GetFrame()];

        return sounds.sounds.Contains(resource.name);
    }

    public bool ToggleSoundResource(SoundResource resource)
    {
        var sounds = editScene.sounds[GetFrame()];

        if (FrameContainsSound(resource))
        {
            sounds.sounds.Remove(resource.name);

            return false;
        }
        else
        {
            sounds.sounds.Add(resource.name);

            return true;
        }
    }

    public void ReplaceImageResource(ImageResource resource)
    {
        selectedImage.text = false;
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

        graphic.keyframes.Add(new Model.KeyFrame
        {
            position = screen * 0.5f,
            scale = scale,
            direction = 0,
        });

        graphic.SetFrameCount(editScene.frameCount);

        selectedImage = graphic;

        scene.Refresh();
    }

    private void PlayRealScene(int id)
    {
        PlayScene(story.scenes[id + 1]);
        previewMode = false;
        timelineSlider.value = 0;
        PlayFrameSounds(0);
    }

    public void CreateBlankStory(string name)
    {
        var story = new Model.Story
        {
            name = name,
            scenes = new List<Model.Scene>(),
        };

        SaveStory(story);
        stories.SetActive(GetStories());
    }

    private IEnumerator Start()
    {
        sensor.OnTokenClassified += token =>
        {
            if (playingMode)
            {
                PlayRealScene(token.id);
            }
        };

        sensor.OnTokenLifted += () =>
        {
            if (playingMode)
            {
                PlayRealScene(-1);
            }
        };

        scenes = scenesSetup.Finalise<Model.Scene>();
        stories = storiesSetup.Finalise<string>();
        images = imagesSetup.Finalise<ImageResource>();
        sounds = soundsSetup.Finalise<SoundResource>();

        if (GetStories().Count() == 0)
        {
            for (int i = 0; i < 3; ++i)
            {
                CreateBlankStory("Blank Story " + (i + 1));
            }
        }

        loadingScreen.SetActive(true);

        string root = "/storage/emulated/0/Download/Creator Images";

#if UNITY_EDITOR
        root = @"C:\Users\mark\Documents\BUILDS\flipology-uploader\files to upload\mark";
#endif

        System.IO.Directory.CreateDirectory(root);

        loadingSlider.value = 0;
        loadingSlider.maxValue = 0;

        loadingSlider.maxValue = System.IO.Directory.GetFiles(root).Length;

        foreach (string file in System.IO.Directory.GetFiles(root))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            string type = Path.GetExtension(file);

            //ThreadedJob.Run<ThreadedReadBytes>(read => read.path = file,
                                               //read =>
            {
                //texture.LoadImage(read.data, true);

                var request = new WWW("file://" + file);

                yield return request;

                if (type == ".png" || type == ".jpg")
                { 
                    var texture = new Texture2D(1, 1);
                    texture.LoadImage(request.bytes, true);

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
                }
                else if (type == ".wav" || type == ".ogg")
                {
                    var clip = request.GetAudioClip(false, false);

                    soundResources.Add(name, new SoundResource
                    {
                        name = name,
                        sound = clip,
                    });
                }

                loadingSlider.value += 1;
            }//);

            yield return null;
        }

        loadingScreen.SetActive(false);
        images.SetActive(imageResources);
        sounds.SetActive(soundResources.Values);

        yield return null;

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

    private int GetFrameFromTime(float time)
    {
        int count = editScene.frameCount;
        int frame = Mathf.CeilToInt(time)
                  + count;

        frame %= count;

        return frame;
    }

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
        
        initialPosition = selectedImage.keyframes[frame].position;
    }

    private void OnPositionDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        selectedImage.keyframes[frame].position = initialPosition + displacement;
    }

    private void OnRotationDragBegin()
    {
        int frame = GetFrame();

        initialRotation = selectedImage.keyframes[frame].direction;
    }

    private void OnRotationDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        selectedImage.keyframes[frame].direction = initialRotation + displacement.y * 0.001f * 360;
    }

    private void OnScalingDragBegin()
    {
        int frame = GetFrame();

        initialScaling = selectedImage.keyframes[frame].scale;
    }

    private void OnScalingDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        selectedImage.keyframes[frame].scale = initialScaling * Mathf.Pow(4, displacement.y * 0.001f);
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

        selectedImage.keyframes[next] = Model.KeyFrame.Copy(selectedImage.keyframes[prev]);

        timelineSlider.value = next;
    }

    public void CopyForwardToEndSelected()
    {
        int prev = GetFrame();

        for (int i = prev; i < editScene.frameCount; ++i)
        {
            selectedImage.keyframes[i] = Model.KeyFrame.Copy(selectedImage.keyframes[prev]);
        }
    }

    public void PreviewScene()
    {
        PlayScene(editScene);
        selectedImage = null;
    }

    public void StopPreview()
    {
        sceneCreatorHUD.SetActive(true);
        previewMode = false;
        timelineSlider.value = Mathf.Floor(timelineSlider.value);
    }

    public void PlayScene(Model.Scene scene)
    {
        OpenScene(scene);

        sceneCreatorHUD.SetActive(false);

        previewMode = true;
    }

    public float fps = 4;

    public void AddFrame()
    {
        editScene.SetFrameCount(editScene.frameCount + 1);
    }

    public void SubFrame()
    {
        editScene.SetFrameCount(editScene.frameCount - 1);
    }

    private void PlayFrameSounds(int frame)
    {
        foreach (var sound in editScene.sounds[GetFrame()].sounds)
        {
            audioSource.PlayOneShot(soundResources[sound].sound);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q))
        {
            ExitStory();
        }

        if (scene.config == null)
            return;

        soundCountText.text = editScene.sounds[GetFrame()].sounds.Count.ToString();

        timelineSlider.maxValue = editScene.frameCount;

        frameCount.text = string.Format("{0} Key Frames @ {1} KFPS ({2} seconds)", editScene.frameCount, fps, (editScene.frameCount + 1) / fps);

        if (previewMode || playingMode)
        {
            float prev = timelineSlider.value;
            float time = prev;
            time += Time.deltaTime * fps;
            time %= timelineSlider.maxValue;
            timelineSlider.value = time;

            int prevFrame = GetFrameFromTime(prev);
            int nextFrame = GetFrameFromTime(time);

            while (prevFrame != nextFrame)
            {
                prevFrame = (prevFrame + 1) % editScene.frameCount;

                PlayFrameSounds(prevFrame);
            }

            selectedImage = null;
        }

        scene.SetFrame(timelineSlider.value);

        if (playingMode && sensor.history.Count > 0)
        {
            var frame_ = sensor.history.Last();
            var offset = new Vector2(Screen.width, Screen.height) * 0.5f;

            ghostReference.position = frame_.position * Screen.dpi / 2.54f;
            ghostReference.eulerAngles = Vector3.forward * frame_.direction;

            scene.images.MapActive((i, view) =>
            {
                if (view.config.ghost)
                {
                    Vector3 position = (Vector3) offset - view.transform.position;

                    view.transform.position = ghostReference.TransformPoint(position);
                    view.transform.Rotate(Vector3.forward, frame_.direction);
                }
            });
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
        bool valid = raycasts.Count == 0 && !playingMode;

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

            if (selectedImage != null)
            {
                ghostToggle.isOn = selectedImage.ghost;
            }

            toolbarObject.SetActive(true);
        }

        if (selectedImage != null)
        {
            selectedImage.ghost = ghostToggle.isOn;
        }

        scene.images.MapActive((i, view) =>
        {
            view.selected = (view.config == selectedImage);
        });
    }
}
