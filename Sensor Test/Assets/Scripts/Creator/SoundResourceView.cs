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
            test.SetSoundResource(config);
        });
    }

    protected override void Configure()
    {
        base.Configure();

        nameText.text = config.name;
    }
}
