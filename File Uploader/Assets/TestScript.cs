using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;
using Encoding = System.Text.Encoding;

public class TestScript : MonoBehaviour 
{
    [SerializeField]
    private RawImage display;
    [SerializeField]
    private Texture2D uploadable;

    private IEnumerator Start()
    {
        var storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
        var root = storage.GetReferenceFromUrl("gs://bearabouts.appspot.com");

        var image = root.Child("CkMualUWUAAci2_.jpg");
        var image2 = root.Child("test.jpg");

        var texture = new Texture2D(1, 1);

        image2.GetBytesAsync(1 * 1024 * 1024)
             .ContinueWith(task =>
             {
                 if (task.IsCompleted)
                 {
                     texture.LoadImage(task.Result);
                     display.texture = texture;
                     display.SetNativeSize();
                 }
                 else
                 {
                     Debug.LogError("DaMN");
                 }
             });

        image2.PutBytesAsync(uploadable.EncodeToJPG())
              .ContinueWith(task =>
              {
                  if (task.IsCompleted)
                  {
                      Debug.Log("good job bb");
                  }
                  else
                  {
                      Debug.LogError("sorry");
                  }
              });

        yield break;
    }
}
