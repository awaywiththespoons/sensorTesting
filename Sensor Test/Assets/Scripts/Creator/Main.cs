﻿using UnityEngine;
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
    private Toggle hideToggle;

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

    private Dictionary<string, ImageResource> imageResources 
        = new Dictionary<string, ImageResource>();
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
        previewMode = false;
        menuCanvas.gameObject.SetActive(true);
        audioSource.Stop();

        Rewind();
    }

    public void ClickedAddText()
    {
        if (replaceMode)
        {
            if (!selectedImage.text)
            {
                selectedImage.path = "text";
            }

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
            SetTogglesFromSelectedFrame();

            scene.Refresh();
        }

        GetInput(selectedImage.path, OnChanged: text => selectedImage.path = hiddenInputField.text);
    }

    private void SetTogglesFromSelectedFrame()
    {
        ghostToggle.isOn = selectedImage.ghost;
        hideToggle.isOn = selectedImage.keyframes[GetFrame()].hide;
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

    public void RenameScene(Model.Scene scene)
    {
        GetInput(scene.name, change => { scene.name = change; scenes.Refresh(); }, change => SaveStory(story));
    }

    public void OpenScene(Model.Scene scene)
    {
        editScene = scene;

        timelineSlider.value = 0;

        this.scene.SetConfig(scene);
        this.scene.Refresh();
    }

    public void OpenStory(string name)
    {
        System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/stories/");

        string path = "";

        try
        {
            path = string.Format("{0}/stories/{1}.json", 
                                 Application.persistentDataPath, 
                                 name);

            string data = System.IO.File.ReadAllText(path);
            story = JsonUtility.FromJson<Model.Story>(data);
        }
        catch (Exception exception)
        {
            Debug.LogFormat("Failed loading '{0}'", path);
            Debug.LogException(exception);
            return;
        }
        
        storyBrowsePanel.SetActive(false);

        story.name = name;
        
        foreach (var scene in story.scenes)
        {
            foreach (var image in scene.images)
            {
                image.sprite = image.text ? clearSprite : imageResources[image.path].sprite;
            }

            // because data may be missing in old save files...
            scene.SetFrameCount(scene.frameCount);
        }

        for (int i = story.scenes.Count; i < 9; ++i)
        {
            var scene = new Model.Scene
            {
                name = "Unnamed",
                images = new List<Model.Image>(),
            };

            story.scenes.Add(scene);

            scene.SetFrameCount(10);
        }

        for (int i = 0; i < 9; ++i)
        {
            story.scenes[i].index = i;
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

    public bool BGSoundMode;

    public bool FrameContainsSound(SoundResource resource)
    {
        var sounds = editScene.sounds[GetFrame()];

        return sounds.sounds.Contains(resource.name);
    }

    public void SetBGMMode()
    {
        BGSoundMode = true;
    }

    public void CancelBGMMode()
    {
        BGSoundMode = false;
    }

    public bool ToggleSoundResource(SoundResource resource)
    {
        if (BGSoundMode)
        {
            if (editScene.bgloop != resource.name)
            {
                editScene.bgloop = resource.name;
                PlayBGLoop(resource);
            }
            else
            {
                editScene.bgloop = "";
                audioSource.Stop();
            }

            return false;
        }

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
        SetTogglesFromSelectedFrame();

        scene.Refresh();
    }

    public void Rewind()
    {
        timelineSlider.value = 0;
    }

    private void PlayRealScene(int id)
    {
        PlayScene(story.scenes[id + 1]);
        previewMode = false;
        Rewind();
        PlayFrameSounds(0);
    }

    public void CreateBlankStory(string name)
    {
        if (name == "")
        {
            int i = 1;

            while (true)
            {
                name = "Blank Story " + i;

                string path = string.Format("{0}/stories/{1}.json", 
                                            Application.persistentDataPath,
                                            name);

                i += 1;

                if (!System.IO.File.Exists(path))
                {
                    break;
                }
            }
        }

        var story = new Model.Story
        {
            name = name,
            scenes = new List<Model.Scene>(),
        };

        SaveStory(story);
        stories.SetActive(GetStories());
    }

    private bool debuggingToken;
    [ContextMenu("Test Scene 1")]
    private void TestScene1()
    {
        SetActiveToken(0);
        debuggingToken = true;
    }

    private void SetActiveToken(int id)
    {
        inactivityTime = 0;

        if (playingMode)
        {
            PlayRealScene(id);
        }
    }

    private IEnumerator ImportResources()
    {
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
            string name2 = Path.GetFileName(file);
            string type = Path.GetExtension(file).ToLower();

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

                    var image = new ImageResource
                    {
                        name = name2,
                        sprite = Sprite.Create(texture,
                                               Rect.MinMaxRect(0, 0, texture.width, texture.height),
                                               Vector2.one * 0.5f,
                                               100,
                                               0,
                                               SpriteMeshType.FullRect),
                    };

                    imageResources.Add(name, image);
                    imageResources.Add(name2, image);
                }
                else if (type == ".wav" || type == ".ogg")
                {
                    var clip = request.GetAudioClip(false, false);

                    soundResources[name] = new SoundResource
                    {
                        name = name,
                        sound = clip,
                    };
                }

                loadingSlider.value += 1;
            }//);

            yield return null;
        }

        loadingScreen.SetActive(false);
        images.SetActive(imageResources.Values.Distinct());
        sounds.SetActive(soundResources.Values);
    }

    public void RefreshResources()
    {
        StartCoroutine(ImportResources());
    }

    [Range(0, 25)]
    [SerializeField]
    private float inactivityTimeout = 1f;
    private float inactivityTime;

    private IEnumerator Start()
    {
        sensor.OnTokenClassified += token => SetActiveToken(token.id);
        //sensor.OnTokenLifted += () => PlayRealScene(-1);

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

        yield return StartCoroutine(ImportResources());

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
        SetTogglesFromSelectedFrame();
    }

    public void StepForward()
    {
        timelineSlider.value = Mathf.FloorToInt(timelineSlider.value + 1) % editScene.frameCount;
        SetTogglesFromSelectedFrame();
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

        StepForward();
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
        Rewind();
        audioSource.Stop();
    }

    public void PlayScene(Model.Scene scene)
    {
        OpenScene(scene);

        sceneCreatorHUD.SetActive(false);

        previewMode = true;

        try
        {
            audioSource.clip = soundResources[editScene.bgloop].sound;
            audioSource.Play();
        }
        catch (Exception e)
        {
            Debug.LogFormat("Couldn't play background loop");
            audioSource.Stop();
        }
    }

    public float fps = 4;

    public void AddFrame(int count)
    {
        editScene.SetFrameCount(editScene.frameCount + count);
    }

    public void SubFrame(int count)
    {
        editScene.SetFrameCount(Math.Max(1, editScene.frameCount - count));
    }

    private void PlayFrameSounds(int frame)
    {
        foreach (var sound in editScene.sounds[GetFrame()].sounds)
        {
            audioSource.PlayOneShot(soundResources[sound].sound);
        }
    }

    public void GetInput(string prev, 
                         Action<string> OnChanged = null,
                         Action<string> OnComplete = null)
    {
        hiddenInputField.onValueChanged.RemoveAllListeners();
        hiddenInputField.onEndEdit.RemoveAllListeners();

        hiddenInputField.text = prev;

        if (OnChanged != null)
        {
            hiddenInputField.onValueChanged.AddListener(text => OnChanged(text));
        }

        if (OnComplete != null)
        {
            hiddenInputField.onEndEdit.AddListener(text => OnComplete(text));
        }

        hiddenInputField.Select();
    }

    public void RenameStory(string name)
    {
        GetInput(name, OnComplete: text =>
        {
            string prev = string.Format("{0}/stories/{1}.json",
                                        Application.persistentDataPath,
                                        name);
            string next = string.Format("{0}/stories/{1}.json",
                                        Application.persistentDataPath,
                                        text);

            try
            {
                System.IO.File.Move(prev, next);
            }
            catch (Exception e)
            {
                Debug.LogFormat("Couldn't rename!");
                Debug.LogException(e);
            }

            stories.SetActive(GetStories());
        });
    }

    public void DuplicateStory(string name)
    {
        string prev = string.Format("{0}/stories/{1}.json",
                                    Application.persistentDataPath,
                                    name);
        string next = prev;

        while (System.IO.File.Exists(next))
        {
            name += "-c";

            next = string.Format("{0}/stories/{1}.json",
                                 Application.persistentDataPath,
                                 name);
        }

        try
        {
            System.IO.File.Copy(prev, next);
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("Couldn't copy '{0}' to '{1}' during copy", prev, next);
            Debug.LogException(e);
        }

        stories.SetActive(GetStories());
    }

    public void RemoveStory(string name)
    {
        string del = string.Format("{0}/stories/deleted/",
                                   Application.persistentDataPath);

        string prev = string.Format("{0}/stories/{1}.json",
                                    Application.persistentDataPath,
                                    name);
        string next = string.Format("{0}/stories/deleted/{1}-{2}.json",
                                    Application.persistentDataPath,
                                    name,
                                    DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));

        System.IO.Directory.CreateDirectory(del);

        try
        {
            System.IO.File.Move(prev, next);
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("Couldn't move '{0}' to '{1}' during delete", prev, next);
            Debug.LogException(e);
        }

        stories.SetActive(GetStories());
    }

    public void PlayBGLoop(SoundResource resource)
    {
        audioSource.clip = resource.sound;
        audioSource.Play();
    }

    public Vector2 tokenPosition;
    [Range(0, 360)]
    public float tokenDirection;

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
            if (playingMode && sensor.detected == null)
            {
                if (inactivityTime < inactivityTimeout)
                {
                    inactivityTime += Time.deltaTime;
                }
                else if (editScene.index != 0)
                {
                    PlayRealScene(-1);
                }
            }

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
        else
        {
            timelineSlider.value = Mathf.Round(timelineSlider.value) % timelineSlider.maxValue;
        }

        scene.SetFrame(timelineSlider.value);

        if (playingMode)
        {
            if (sensor.history.Any())
            {
                var frame_ = sensor.history.Last();
                tokenPosition = frame_.position * Screen.dpi / 2.54f;
                tokenDirection = frame_.direction;
            }
            
            var offset = new Vector2(Screen.width, Screen.height) * 0.5f;

            ghostReference.position = tokenPosition;
            ghostReference.eulerAngles = Vector3.forward * tokenDirection;

            scene.images.MapActive((i, view) =>
            {
                if (view.config.ghost)
                {
                    Vector3 position = (Vector3) offset - view.transform.position;

                    view.transform.position = tokenPosition - (Vector2) (ghostReference.rotation * position);
                    view.transform.Rotate(Vector3.forward, tokenDirection);
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
                SetTogglesFromSelectedFrame();
            }

            toolbarObject.SetActive(true);
        }

        if (selectedImage != null)
        {
            selectedImage.ghost = ghostToggle.isOn;
            selectedImage.keyframes[GetFrame()].hide = hideToggle.isOn;
        }

        scene.images.MapActive((i, view) =>
        {
            view.selected = (view.config == selectedImage);
        });
    }
}
