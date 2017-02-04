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

    public struct TouchPattern
    {
        public int count;
        public Vector2 a, b, c;
    }

    public struct Frame
    {
        public Token token;
        public Vector2 position;
        public float direction;

        public List<TouchPattern> touches;
    }

    public event Action OnTokenPlaced = delegate { };
    public event Action OnTokenLifted = delegate { };
    public event Action<Frame> OnTokenClassified = delegate { };
    public event Action<Frame> OnTokenTracked = delegate { };

    public static float PolarAngle(Vector2 point)
    {
        return Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg;
    }

    private Token training;

    public TouchPattern touchPattern;

    public List<Token> knownTokens = new List<Token>();
    private Queue<Frame> history = new Queue<Frame>();

    public void SetTraining(Token token)
    {
        knownTokens.Add(token);
        training = token;
    }

    public void Reset()
    {
        training = null;
        knownTokens.Clear();
        history.Clear();
    }

    private void Update()
    {
        // get touches, scale to centimetres and sort clockwise
        touchPattern.count = Input.touchCount;
        touchPattern.a = touchPattern.count >= 1 ? Input.GetTouch(0).position / Screen.dpi * 2.54f : default(Vector2);
        touchPattern.b = touchPattern.count >= 2 ? Input.GetTouch(1).position / Screen.dpi * 2.54f : default(Vector2);
        touchPattern.c = touchPattern.count >= 3 ? Input.GetTouch(2).position / Screen.dpi * 2.54f : default(Vector2);

        SortPatternClockwise(ref touchPattern);

        if (training != null)
        {
            // if we're training and have exactly three touches, add this
            // pattern as an example of the token we're training
            if (touchPattern.count == 3)
            {
                TrainToken(training, touchPattern);
            }
        }
        else
        {
        }
    }

    /// <summary>
    /// Add an example of touch pattern that is seen when the given token is
    /// on screen
    /// </summary>
    public void TrainToken(Token token, TouchPattern pattern)
    {
        Vector3 feature = ExtractSidesFeature(pattern);

        if (token.training.Count == 0)
        {
            token.training.Add(feature);
        }
        else
        {
            token.training.Add(Triangle.CycleToMatch(feature, token.training.Last()));
        }
    }

    /// <summary>
    /// sort the position in the touch pattern so the corners of the triangle
    /// occur in a clockwise order
    /// </summary>
    public static void SortPatternClockwise(ref TouchPattern pattern)
    {
        Vector2 centroid = (pattern.a + pattern.b + pattern.c) / 3f;

        var touches = new List<Vector2> { pattern.a, pattern.b, pattern.c };
        var sorted = touches.OrderBy(touch => -PolarAngle(touch - centroid)).ToList();

        pattern.a = sorted[0];
        pattern.b = sorted[1];
        pattern.c = sorted[2];
    }

    /// <summary>
    /// Convert a pattern of three touches into a Vector3 of side lengths of a
    /// triangle
    /// </summary>
    public Vector3 ExtractSidesFeature(TouchPattern pattern)
    {
        var triangle = new Vector3((pattern.b - pattern.a).magnitude,
                                   (pattern.c - pattern.b).magnitude,
                                   (pattern.a - pattern.c).magnitude);

        // line-like triangles don't have a reliable concept of clockwise
        // winding, so instead we sort the sides by length to normalise the
        // feature
        if (Triangle.IsLine(triangle))
        {
            triangle = Triangle.SortSides(triangle);
        }

        return triangle;
    }

    /// <summary>
    /// TODO: annihilate training points that are too close to each other but
    /// representing different tokens
    /// </summary>
    public void ReduceNoise()
    {

    }
}
