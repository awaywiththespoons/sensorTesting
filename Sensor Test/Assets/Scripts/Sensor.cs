using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class Sensor : MonoBehaviour 
{
    public class Token
    {
        public int id;

        public List<Vector3> training = new List<Vector3>();
    }

    public struct Frame
    {
        public Token token;
        public Vector2 position;
        public float direction;

        public List<Vector2> touches;
    }

    public event Action OnTokenPlaced = delegate { };
    public event Action OnTokenLifted = delegate { };
    public event Action<Frame> OnTokenClassified = delegate { };
    public event Action<Frame> OnTokenTracked = delegate { };

    private Token training;

    private List<Token> knownTokens = new List<Token>();
    private Queue<Frame> history = new Queue<Frame>();

    public void Reset()
    {
        knownTokens.Clear();
        history.Clear();
    }

    private void Update()
    {
        if (training != null)
        {

        }
        else
        {

        }
    }
}
