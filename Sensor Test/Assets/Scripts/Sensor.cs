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

        public TouchPattern pattern;
    }

    private struct Data
    {
        public Token token;
        public Vector3 feature;
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
    private Token detected;

    private float tokenTime;
    private float tokenTimeout;
    private List<Token> classifications = new List<Token>();

    public List<Token> knownTokens = new List<Token>();
    private Queue<Frame> history = new Queue<Frame>();
    private List<Data> allTraining = new List<Data>();

    public void SetTraining(Token token)
    {
        knownTokens.Add(token);
        training = token;
    }

    public void SetClassify()
    {
        training = null;
        classifications.Clear();
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
        TouchPattern pattern;
        pattern.count = Input.touchCount;
        pattern.a = pattern.count >= 1 ? Input.GetTouch(0).position / Screen.dpi * 2.54f : default(Vector2);
        pattern.b = pattern.count >= 2 ? Input.GetTouch(1).position / Screen.dpi * 2.54f : default(Vector2);
        pattern.c = pattern.count >= 3 ? Input.GetTouch(2).position / Screen.dpi * 2.54f : default(Vector2);

        IntegrateTouchPattern(pattern);
        
        if (pattern.count < 2 || pattern.count > 3)
        {
            tokenTimeout += Time.deltaTime;
        }
        else
        {
            tokenTimeout = 0;
        }

        if (tokenTimeout > .25f)
        {
            detected = null;
            // TODO: reset classification too

            OnTokenLifted();
        }
    }

    public void IntegrateTouchData(IList<Vector2> touches)
    {
        TouchPattern pattern;
        pattern.count = touches.Count;
        pattern.a = pattern.count >= 1 ? touches[0] / Screen.dpi * 2.54f : default(Vector2);
        pattern.b = pattern.count >= 2 ? touches[1] / Screen.dpi * 2.54f : default(Vector2);
        pattern.c = pattern.count >= 3 ? touches[2] / Screen.dpi * 2.54f : default(Vector2);

        IntegrateTouchPattern(pattern);
    }

    public void IntegrateTouchPattern(TouchPattern pattern)
    {
        SortPatternClockwise(ref pattern);

        if (training != null)
        {
            // if we're training and have exactly three touches, add this
            // pattern as an example of the token we're training
            if (pattern.count == 3)
            {
                TrainToken(training, pattern);
            }
        }
        else if (detected != null)
        {
            // if we have already detected a token, we can try to reconstruct
            // the missing touch from the known points and expected shape
            if (pattern.count == 2)
            {
                // TODO: reconstruct the missing touch - use history to 
                // determine which possible reconstruction is most likely
            }

            // limit history size
            // TODO: actually use history to assist orientation
            while (history.Count >= 16)
            {
                history.Dequeue();
            }
            
            history.Enqueue(OrientToken(detected, pattern));

            OnTokenTracked(history.Last());
        }
        else if (pattern.count == 3)
        {
            Token classification = ClassifyPattern(pattern);

            if (classification != null)
            {
                classifications.Add(classification);

                Debug.Log(classification.id);
            }
            else
            {
                Debug.Log("???");
            }

            if (classifications.Count >= 16)
            {
                var best = knownTokens.OrderByDescending(token => classifications.Count(data => data == token))
                                      .First();

                detected = best;

                Debug.Log("BEST IS " + best.id);

                //OnTokenClassified(history.Last());
            }
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

        allTraining.Add(new Data { token = token, feature = feature });
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
    /// 
    /// also kill training points that don't have any friends nearby?
    /// 
    /// maybe we can assume a single cluster - kill anything too far from the
    /// mean?
    /// </summary>
    public void ReduceNoise()
    {

    }

    /// <summary>
    /// Return a frame containing the token type, touch pattern, estimated 
    /// direction and estimated position
    /// </summary>
    public Frame OrientToken(Token token, TouchPattern pattern)
    {
        // TODO: determine direction from pattern/token
        return new Frame
        {
            token = token,
            position = (pattern.a + pattern.b + pattern.c) / 3f,
            direction = 0,

            pattern = pattern,
        };
    }

    /// <summary>
    /// Return the known token most likely to be responsible for this touch 
    /// pattern. If none are close, return null
    /// </summary>
    public Token ClassifyPattern(TouchPattern pattern)
    {
        Vector3 feature = ExtractSidesFeature(pattern);

        var closest = allTraining.OrderBy(data => Triangle.CompareBestCycle(data.feature, feature))
                                 .Take(16)
                                 .ToList();
        
        var best = knownTokens.OrderByDescending(token => closest.Count(data => data.token == token))
                              .First();

        return best;
    }
}
