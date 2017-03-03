using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class Test : MonoBehaviour 
{
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

    private void Update()
    {
        if (play)
        {
            timelineSlider.value = (timelineSlider.value + Time.deltaTime * fps * playbackSpeed) % data.frameCount;
        }

        int frame = Mathf.CeilToInt(timelineSlider.value);

        frame %= data.images[0].frameCount;

        if (Input.GetMouseButton(0))
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
