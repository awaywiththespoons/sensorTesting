using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class AnimateSpritesSimply : MonoBehaviour 
{
    SpriteRenderer sprites;
    Image images;

    bool useSpriteRenderer = true;

    public int current = 0;
    public List<Sprite> frames;
    Sprite lastFirstSprite;
    public float framesPS = 1.8f;
    float lastFPS;

    float count= 0;
    float length = 100000000000;

    public bool randomStart = false;
    public bool renameToTexture;
    public bool updateNow = false;

    void Awake()
    {
        if (GetComponent<SpriteRenderer>() == null)
        {
            images = GetComponent<Image>();
            useSpriteRenderer = false;
        }
        
        Sprite _temp;
        if (useSpriteRenderer)
            _temp = GetComponent<SpriteRenderer>().sprite;
        else
            _temp = GetComponent<Image>().sprite;

        ResetFrames();
        if(frames[0] == null)
        frames[0] = _temp;
    }

    void ResetFrames()
    {
        if (frames == null)
            frames = new List<Sprite>();

        if (frames.Count == 0)
        {
            frames = new List<Sprite>();
            frames.Add(null);
            frames.Add(null);
        }
    }

    void Start ()
    {

        lastFPS = framesPS;

        length = 1 / (float)framesPS;
        sprites = GetComponent<SpriteRenderer>();

        if (randomStart)
            count = Random.Range(0, length);

        if (current >= frames.Count)
            current = 0;

        if (frames == null || frames.Count == 0)
        {
            ResetFrames();
        }

        UpdateSprite();
	}

    void UpdateSprite()
    {
        if (useSpriteRenderer)
            sprites.sprite = frames[current];
        else
            images.sprite = frames[current];

    }

	void Update ()
    {
        if (renameToTexture)
            RenameToTexture();

        if (lastFirstSprite != frames[0])
        {
            lastFirstSprite = frames[0];
            updateNow = true;
        }

        if (updateNow)
        {
            updateNow = false;
            UpdateSprite();
        }

        if(lastFPS != framesPS)
        {
            lastFPS = framesPS;
            length = 1 / (float)framesPS;
        }

        count += Time.deltaTime;
        if(count > length)
        {
            count -= length;
            current++;

            if (current >= frames.Count)
                current = 0;

            UpdateSprite();
        }
	}


    void RenameToTexture()
    {
        renameToTexture = false;

        if (GetComponent<MeshRenderer>() == null && GetComponent<SpriteRenderer>() == null)
            return;

       // string _name = "";

        TextMesh _text = GetComponent<TextMesh>();
        SpriteRenderer _sprite = GetComponent<SpriteRenderer>();

        if (_text != null)
        {
            if (_text.text.Length > 26)
                transform.name = "" + GetComponent<TextMesh>().text.Substring(0, 26);
            else
                transform.name = "" + GetComponent<TextMesh>().text;
            return;
        }
        else if (_sprite != null)
        {
            if(_sprite.sprite.texture.name.Length > 26)
            {
                transform.name = "" + _sprite.sprite.name.Substring(0, 26);

            }
            else
            {
                transform.name = "" + _sprite.sprite.name;

            }
        }

    }
}
