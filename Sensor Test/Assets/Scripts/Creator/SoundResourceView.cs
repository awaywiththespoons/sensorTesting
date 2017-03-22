using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class SoundResourceView : InstanceView<Main.SoundResource> 
{
    [SerializeField]
    private Main test;
    [SerializeField]
    private Button selectButton;
    [SerializeField]
    private GameObject selectedObject;
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private AudioSource source;

    private void Start()
    {
        selectButton.onClick.AddListener(() =>
        {
            source.Stop();
            source.clip = config.sound;
            source.Play();
            test.ToggleSoundResource(config);
            Update();
        });
    }

    protected override void Configure()
    {
        base.Configure();

        nameText.text = config.name;
    }
    
    private void Update()
    {
        bool active = (test.BGSoundMode && test.editScene.bgloop == config.name)
                   || (!test.BGSoundMode && test.FrameContainsSound(config));
        selectedObject.SetActive(active);
    }
}
