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
    private Slider timelineSlider;

    [SerializeField]
    private Canvas sceneCanvas;

    [SerializeField]
    private SceneView scene;

    [SerializeField]
    private Model.Scene data;

    [SerializeField]
    private bool play;
    [SerializeField]
    private bool angle;

    [SerializeField]
    [Range(0, 1)]
    private float playbackSpeed = 1;

    private void Start()
    {
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

    private int GetFrame()
    {
        int frame = Mathf.CeilToInt(timelineSlider.value);

        frame %= data.images[0].frameCount;

        return frame;
    }

    private void OnPositionDragBegin()
    {
        int frame = GetFrame();

        draggingPosition = true;
        initialPosition = data.images[0].positions[frame];
    }

    private void OnPositionDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        data.images[0].positions[frame] = initialPosition + displacement;
    }

    private void OnPositionDragEnd()
    {
        draggingPosition = false;
    }

    private void OnRotationDragBegin()
    {
        int frame = GetFrame();

        draggingRotation = true;
        initialRotation = data.images[0].directions[frame];
    }

    private void OnRotationDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        data.images[0].directions[frame] = initialRotation + displacement.y * 0.001f * 360;
    }

    private void OnRotationDragEnd()
    {
        draggingRotation = false;
    }

    private void OnScalingDragBegin()
    {
        int frame = GetFrame();

        draggingScaling = true;
        initialScaling = data.images[0].scales[frame];
    }

    private void OnScalingDragChange(Vector2 displacement)
    {
        int frame = GetFrame();

        data.images[0].scales[frame] = initialScaling + displacement.y * 0.001f;
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

    private void Update()
    {
        if (positionDrag.dragging || rotationDrag.dragging || scalingDrag.dragging)
        {
            fadeGroup.alpha = 0.01f;
        }
        else
        {
            fadeGroup.alpha = 1f;
        }

        var pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        raycasts.Clear();
        raycaster.Raycast(pointer, raycasts);
        bool valid = raycasts.Count == 0;

        if (play)
        {
            timelineSlider.value = (timelineSlider.value + Time.deltaTime * fps * playbackSpeed) % data.frameCount;
        }

        int frame = GetFrame();

        if (Input.GetMouseButton(0) && valid)
        {
            if (angle)
            {
                var center = new Vector2(Camera.main.pixelWidth / 2,
                                         Camera.main.pixelHeight / 2);

                float max = Mathf.Min(center.x, center.y);

                Vector2 direction = (Vector2) Input.mousePosition - center;

                float scale = Mathf.Min(direction.magnitude, max);
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                data.images[0].directions[frame] = angle;
                data.images[0].scales[frame] = scale / max;
            }
            else
            {
                data.images[0].positions[frame] = Input.mousePosition;
            }
        }

        scene.SetFrame(timelineSlider.value);
    }
}
